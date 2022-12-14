using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Sim3D_Legacy;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.Cycles;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation
{
    public static class Sim3D
    {
        public static readonly Dictionary<int, int[]> CachedBlocks =
            new Dictionary<int, int[]>(Globals.Tool.Vertices.Count);

        private static readonly Dictionary<string, MethodInfo> CachedMethods = new Dictionary<string, MethodInfo>().Do(
            d =>
            {
                if (CustomAssemblyManager.Self is null)
                {
                    throw new NotSupportedException("CustomAssemblyManager was null when SIM3D was trying to access it!");
                }
                var asses = CustomAssemblyManager.Self.ImportedAssemblies;
                foreach (var assembly in asses)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            try
                            {
                                if (method.GetCustomAttribute<CustomMethodAttribute>() != null)
                                {
                                    d.Add(method.Name, method);
                                }
                            }
                            catch
                            {
                                Globals.Console.Push($"Failed to add '{method.Name}' to the dictionary, Key Already exists!");
                            }
                        }
                    }
                }
                
                Globals.Console.Push(new string[]
                {
                    $"Added {d.Count.ToString()} methods to the cached dictionary.\n    Methods:"
                }.Concat(d.Keys).ToArray());
                return d;
            });
        /// <summary>
        /// Stalls the next action (ICommand) asynchronously until the previous one completes.
        /// </summary>
        /// <param name="tool">The tool.</param>
        public static async Task WaitUntilActionIsValid(this ITool tool)
        {
            var toolValues = tool.Values;
            if (!toolValues.IsAllowed || toolValues.IsPaused)
            {
                Globals.Console.Push("Waiting...");
                Hourglass hourglass = Hourglass.GetOrCreate(nameof(WaitUntilActionIsValid));
                bool isEven = true;
                while (!toolValues.IsAllowed || toolValues.IsPaused)
                {
                    if (toolValues.IsReset)
                    {
                        return;
                    }

                    if (isEven)
                    {
                        await Task.Yield();
                        isEven = false;
                    }
                    else
                    {
                        isEven = true;
                    }
                    await Task.Delay(toolValues.FastMoveTick, toolValues.TokenSource.Token);
                }
                hourglass.Dispose();
                Globals.Console.Push($"Exited: {nameof(WaitUntilActionIsValid)}!");
            }
        }
        /// <summary>
        /// Sets the tool's current destination and path.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="points">The tool's current path.</param>
        public static void SetupTranslation(this ITool tool, Vector3[] points)
        {
            var toolValues = tool.Values;
            toolValues.Destination = new Target(points.Last());
            toolValues.CurrentPath = new Path(points);
        }
        /// <summary>
        /// Draws a line from the previous point to the current/next for x seconds.
        /// </summary>
        public static void DrawTranslation(this bool draw, Vector3 previous, Vector3 next, float duration = 10f)
        {
            if (draw)
            {
                Debug.DrawLine(previous, next, new Color(255, 0, 0), duration);
            }
        }
        /// <summary>
        /// Checks whether the vertex provided is within the mesh's bounds.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="vertex">A point in 3D space.</param>
        /// <returns>Whether the vertex provided is within the mesh's bounds.</returns>
        public static bool IsOkayToCutVertex(this ITool tool, Vector3 vertex)
        {
            var d = new Direction();
            vertex = tool.Cube.transform.TransformVector(vertex);
            if (vertex.x < tool.MinX || vertex.x > tool.MaxX)
            {
                d.X = vertex.x.Abs();
            }
            if (vertex.y < tool.MinY || vertex.y > tool.MaxY)
            {
                d.Y = vertex.y.Abs();
            }
            if (vertex.z < tool.MinZ || vertex.z > tool.MaxZ)
            {
                d.Z = vertex.z.Abs();
            }

            var isOk = d.X == 0 && d.Y == 0 && d.Z == 0;
            return isOk;
        }
        
        
        /// <summary>
        /// Checks if the tool has come in (radius) distance of some vertex in the <see cref="ITool.Cube"/>'s mesh, if it has then it attempts to 'cut' it.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="direction">The direction of the tool (The way it is cutting).</param>
        /// <param name="throwIfCut">A boolean defining whether the exception below is going to be thrown or not due to the <see cref="G00"/> command.</param>
        /// <returns>An asynchronous task resulting in a <see cref="CutResult"/> statistic, defining time spent cutting and total vertices cut.</returns>
        /// <exception cref="RapidFeedCollisionException">This exception is thrown if the command used for the execution of this action is of type <see cref="G00"/>,
        /// causing a rapid feed collision (High speed hit into the workpiece).</exception>
        public static CutResult CheckPositionForCut(this ITool tool, Direction direction, bool throwIfCut)
        {
            if (CachedMethods.ContainsKey("CheckPositionForCut"))
            {
                using Hourglass hourglass = Hourglass.GetOrCreate("CheckPositionForCut_Custom");
                if (hourglass.Finishing is null)
                {
                    hourglass.Finishing = hg =>
                    {
                        Globals.Console.Push($"[Hourglass] - Method '{hg.Name}' took {hg.Stopwatch.Elapsed.TotalMilliseconds.Round(3)}ms to complete.");
                    };
                }
                var method = CachedMethods["CheckPositionForCut"];
                return (CutResult) method.Invoke(null, new object[]
                {
                    tool,
                    direction,
                    throwIfCut
                });
            }
            //we could parse the cube once, and map all points that are close to the central point and then use those for all calculations?
            Stopwatch stopwatch = Stopwatch.StartNew();
            long verticesCut = 0;
            var pos = tool.Position;
            var tr = tool.Cube.transform;
            
            var vertices = tool.Vertices;
            var trVT = tool.Temp.transform;
            var trV = trVT.position;
            var radius = tool.Values.Radius;
            var isDrilling = tool.Values.Current.GetType() == typeof(CYCLE81);
            for (int i = 0; i < vertices.Count; i++)
            {
                var vert = vertices[i];
                var realVert = tr.TransformPoint(vert);
                var distVertical = Space3D.Distance(trV.y, realVert.y);
                var distHorizontal = Vector2.Distance(new Vector2(realVert.x, realVert.z), new Vector2(pos.x, pos.z));
                if (distHorizontal <= tool.ToolConfig.Radius &&
                    distVertical <= tool.ToolConfig.VerticalMargin)
                {
                    if (throwIfCut)
                    {
                        stopwatch.Stop();
                        verticesCut++;

                        return new CutResult(stopwatch.ElapsedMilliseconds, verticesCut, true);
                    }
                    
                    tool.Colors[i] = tool.ToolConfig.ToolColor;
                    
                    Line2D line = new Line2D(new Vector2D(realVert.x, realVert.z), new Vector2D(pos.x, pos.z));
                    var vcs = line.FindCircleEndPoint(radius, pos.x, pos.z);
                    var distance1 = Space2D.Distance(vcs.positive, new Vector2D(realVert.x, realVert.z));
                    var distance2 = Space2D.Distance(vcs.negative, new Vector2D(realVert.x, realVert.z));
                    bool flag = distance2 > distance1;
                    Vector2D vector3d = ((flag ? vcs.positive : vcs.negative));
                    vertices[i] = new Vector3(vector3d.x, vert.y - distVertical, vector3d.y);
                    verticesCut++;
                }
                else if (isDrilling && distHorizontal <= tool.ToolConfig.Radius + 2f &&
                         distVertical <= tool.ToolConfig.VerticalMargin)
                {
                    if (throwIfCut)
                    {
                        stopwatch.Stop();
                        verticesCut++;

                        return new CutResult(stopwatch.ElapsedMilliseconds, verticesCut, true);
                    }
                    
                    //tool.Colors[i] = tool.ToolConfig.ToolColor;
                    
                    Line2D line = new Line2D(new Vector2D(realVert.x, realVert.z), new Vector2D(pos.x, pos.z));
                    var vcs = line.FindCircleEndPoint(radius + 1f, pos.x, pos.z);
                    var distance1 = Space2D.Distance(vcs.positive, new Vector2D(realVert.x, realVert.z));
                    var distance2 = Space2D.Distance(vcs.negative, new Vector2D(realVert.x, realVert.z));
                    bool flag = distance2 > distance1;
                    Vector2D vector3d = ((flag ? vcs.positive : vcs.negative));
                    Vector3 vector3 = new Vector3(vector3d.x, vert.y, vector3d.y);
                    vertices[i] = vector3;
                }
            }

            var mesh = tool.Workpiece.Current;
            mesh.vertices = vertices.GetInternalArray();
            mesh.triangles = tool.Triangles.GetInternalArray();
            mesh.colors = tool.Colors.GetInternalArray();
            stopwatch.Stop();

            return new CutResult(stopwatch.Elapsed.TotalMilliseconds, verticesCut);
        }

        [CustomMethod(nameof(TraverseFinal))]
        public static Task TraverseFinal(this ITool tool, Vector3[] points, bool draw, bool logStats)
        {
            var traverseState = Globals.MethodManager.Get("Traverse");
            switch (traverseState.Index)
            {
                case -1:
                {
                    return tool.TraverseForceBased(points, draw, logStats);
                }

                case 0:
                {
                    return tool.Traverse(points, draw, logStats);
                }

                default:
                {
                    if (!CachedMethods.ContainsKey("Traverse"))
                    {
                        throw new MissingMethodException("This method has not been re-implemented yet.");
                    }

                    Task awaitableTask = (Task) CachedMethods["Traverse"].Invoke(null, new object[]{tool, points, draw, logStats});
                    return awaitableTask;
                }
            }
        }
        
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Vector3"/>[].
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="points">The tool's current path.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        [CustomMethod(nameof(Traverse))]
        public static async Task Traverse(this ITool tool, Vector3[] points, bool draw, bool logStats = true)
        {
            var toolValues = tool.Values;
            var throwIfCutIsDetected = toolValues.Current.GetType() == typeof(G00);
            double averageTimeForCut = 0;
            long totalCut = 0;
            tool.SetupTranslation(points);
            var currentPosition = tool.Position;
            var last = points.Last();
            foreach (var point in points)
            {
                draw.DrawTranslation(currentPosition, point);
                tool.Position = point;
                var cutResult = tool.CheckPositionForCut(Direction.FromVectors(currentPosition, point), throwIfCutIsDetected);
                LogCutStatistics(logStats, cutResult,  ref averageTimeForCut, ref totalCut);
                currentPosition = point;
                if (cutResult.Threw)
                {
                    throw new RapidFeedCollisionException(tool.Values.Current);
                }
                await FinishCurrentMove(toolValues);
                if (toolValues.TokenSource.IsCancellationRequested)
                {
                    Globals.Console.Push($"Cancelled task - {toolValues.Current.Id}!");
                    break;
                }

                if (currentPosition == last)
                {
                    break;
                }
            }
            if (toolValues.ExactStopCheck)
            {
                await tool.InvokeOnConsumeStopCheck();
            }
            if (logStats)
            {
                PyroConsoleView.PushTextStatic("Traverse finished!", $"Total vertices cut: {totalCut} ({(((double) totalCut / tool.Vertices.Count) * 100).Round().ToString(CultureInfo.InvariantCulture)}%)",
                                               $"Average time spent cutting: {averageTimeForCut.Round().ToString(CultureInfo.InvariantCulture)}ms");  
            }
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Line3D"/>.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="line">The tool's current path.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ITool tool, Line3D line, bool draw, bool logStats = true)
        {
            await tool.TraverseFinal(line.ToVector3s(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by either a <see cref="Circle"/>, <see cref="Circle3D"/> or <see cref="Arc3D"/>.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="arc">The tool's current path.</param>
        /// <param name="reverse">Whether to reverse the direction of movement for the <see cref="I3DShape"/>.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ITool tool, I3DShape arc, bool reverse, bool draw, bool logStats = true)
        {
            if (reverse)
            {
                arc.Reverse();
            }
            await tool.TraverseFinal(arc.ToVector3s(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by either a <see cref="Circle"/>, <see cref="Circle3D"/> or <see cref="Arc3D"/>.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="circleCenter">The circle's center.</param>
        /// <param name="circleRadius">The circle's radius.</param>
        /// <param name="reverse">Whether to reverse the direction of movement for the <see cref="Circle3D"/>.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ITool tool, Vector3 circleCenter, float circleRadius, bool reverse, bool draw, bool logStats = true)
        {
            var circle3d = new Circle3D(circleRadius, tool.Position.y);
            if (reverse)
            {
                circle3d.Reverse();
            }
            circle3d.Shift(circleCenter.ToVector3D());
            
            await tool.TraverseFinal(circle3d.ToVector3s(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Vector3"/> destination, later converted to a <see cref="Line3D"/>.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="destination">The tool's current destination.</param>
        /// <param name="smoothness">The total amount of points the tool is to traverse through.[overriden]</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ITool tool, Vector3 destination, LineTranslationSmoothness smoothness, bool draw, bool logStats = true)
        {
            var p1 = tool.Position.ToVector3D();
            var p2 = destination.ToVector3D();
            var dist = Space3D.Distance(p1, p2);
            var line = new Line3D(tool.Position.ToVector3D(), destination.ToVector3D(), dist.Mutate(d =>
            {
                if (d == 0f)
                {
                    return 1;
                }
                else if (d < 5)
                {
                    return 10;
                }
                else if (d < 50)
                {
                    return 50;
                }

                return (int) d;
            }));
            await tool.Traverse(line, draw, logStats);
        }
        internal static async Task FinishCurrentMove(ToolValues toolValues)
        {
            await Task.Delay(toolValues.FastMoveTick);
            await Task.Yield();
        }
        internal static void LogCutStatistics(bool logStats, CutResult cutResult, ref double averageTimeForCut, ref long totalCut)
        {
            if (logStats)
            {
                averageTimeForCut = (averageTimeForCut + cutResult.TotalTime) / 2d;
                totalCut += cutResult.TotalVerticesCut;
            }
        }
    }
}