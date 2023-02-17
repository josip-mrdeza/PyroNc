using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Sim3D_Legacy;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.Cycles;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.UI;
using Pyro.Nc.UI.Debug;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Path = Pyro.Nc.Pathing.Path;

namespace Pyro.Nc.Simulation
{
    public static class Sim3D
    {
        public static readonly Dictionary<int, int[]> CachedBlocks =
            new Dictionary<int, int[]>(MachineBase.CurrentMachine.Workpiece.Vertices.Count);

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

        public static readonly Dictionary<int, int> VertexTracker = new Dictionary<int, int>();
        /// <summary>
        /// Sets the tool's current destination and path.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="points">The tool's current path.</param>
        public static void SetupTranslation(this ToolBase toolBase, Vector3[] points)
        {
            var toolValues = toolBase.Values;
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
        /// <param name="toolBase">The tool.</param>
        /// <param name="vertex">A point in 3D space.</param>
        /// <returns>Whether the vertex provided is within the mesh's bounds.</returns>
        public static Direction IsOkayToCutVertex(this ToolBase toolBase, Vector3 vertex)
        {
            /*var d = new Direction();
            //vertex = tool.Cube.transform.TransformVector(vertex);
            if (vertex.x < toolBase.MinX)
            {
                d.X = -vertex.x.Abs();
            }
            else if (vertex.x > toolBase.MaxX)
            {
                d.X = vertex.x;
            }
            //
            if (vertex.y < toolBase.MinY)
            {
                d.Y = -vertex.y;
            }
            else if (vertex.y > toolBase.MaxY)
            {
                d.Y = vertex.y;
            }
            //
            if (vertex.z < toolBase.MinZ)
            {
                d.Z = -vertex.z;
            }
            else if (vertex.z > toolBase.MaxZ)
            {
                d.Z = vertex.z;
            }

            return d;*/
            return default;
        }
        
        //TODO trialob ono za neke radiuse stavit koe su tocke u tom radiusu. i po tome onda radit sve ovo tob trialo bit puuno brze mozda?
        /// <summary>
        /// Checks if the tool has come in (radius) distance of some vertex in the <see cref="ToolBase.Cube"/>'s mesh, if it has then it attempts to 'cut' it.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="direction">The direction of the tool (The way it is cutting).</param>
        /// <param name="throwIfCut">A boolean defining whether the exception below is going to be thrown or not due to the <see cref="G00"/> command.</param>
        /// <returns>An asynchronous task resulting in a <see cref="CutResult"/> statistic, defining time spent cutting and total vertices cut.</returns>
        /// <exception cref="RapidFeedCollisionException">This exception is thrown if the command used for the execution of this action is of type <see cref="G00"/>,
        /// causing a rapid feed collision (High speed hit into the workpiece).</exception>
        public static unsafe CutResult CheckPositionForCut(this ToolBase toolBase, Dictionary<int, bool> dict, Direction direction, bool throwIfCut)
        {
            /*if (CachedMethods.ContainsKey("CheckPositionForCut"))
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
                // return (CutResult) method.Invoke(null, new object[]
                // {
                //     tool,
                //     direction,
                //     throwIfCut
                // });
                method.Invoke(null, new object[]
                {
                     toolBase,
                     direction,
                     throwIfCut
                });

                return default;
            }
            //we could parse the cube once, and map all points that are close to the central point and then use those for all calculations?
            Stopwatch stopwatch = Stopwatch.StartNew();
            long verticesCut = 0;
            var pos = toolBase.Position;
            var tr = toolBase.Cube.transform;
            
            var vertices = toolBase.Vertices;
            var trVT = toolBase.Temp.transform;
            var trV = trVT.position;
            var radius = toolBase.Values.Radius;
            var virtualMap = toolBase.VertexHashmap;
            var vects = new List<Algorithms.VertexMap>();
            foreach (var key in virtualMap.Keys)
            {
                if (key.IsInBox(pos))
                {
                    vects.AddRange(virtualMap[key]);
                }
            }
            var verts = vects.Count;
            for (int i = 0; i < verts; i++)
            {
                if (VertexTracker[i] >= 1)
                {
                    //Globals.Console.Push($": {i} skipped");
                    continue;
                }

                var map = vects[i];
                Globals.Console.Push($"Working at map: ({i}), {map.Index} in vertex array\n-- {map.Vertex.ToString()}");
                var vert = vertices[map.Index];
                var realVert = tr.TransformPoint(vert);
                if (!toolBase.IsOkayToCutVertex(realVert).IsZeroed())
                {
                    stopwatch.Stop();
                    verticesCut++;

                    return new CutResult(stopwatch.ElapsedMilliseconds, verticesCut, true);
                }
                var distVertical = Space3D.Distance(trV.y, realVert.y);
                var distHorizontal = Vector2.Distance(new Vector2(realVert.x, realVert.z), new Vector2(pos.x, pos.z));
                if (distHorizontal <= toolBase.ToolConfig.Radius &&
                    distVertical <= toolBase.ToolConfig.VerticalMargin)
                {
                    dict[i] = true;
                    if (throwIfCut)
                    {
                        stopwatch.Stop();
                        verticesCut++;

                        return new CutResult(stopwatch.ElapsedMilliseconds, verticesCut, true);
                    }

                    var realVector2D = new Vector2D(realVert.x, realVert.z);
                    Line2D line = new Line2D(realVector2D, new Vector2D(pos.x, pos.z));
                    //line = line.ToEdgeAngleCast();
                    var lineEquationResults = line.FindCircleEndPoint(radius, pos.x, pos.z);
                    if (lineEquationResults.ResultOfDiscriminant == Line2D.LineEquationResults.DiscriminantResult.Imaginary)
                    {
                        continue;
                    }
                    Vector2D vector3d;
                    if (lineEquationResults.ResultOfDiscriminant == Line2D.LineEquationResults.DiscriminantResult.One)
                    {
                        vector3d = lineEquationResults.Result1;
                    }
                    else
                    {
                        var distance1 = Space2D.Distance(lineEquationResults.Result1, realVector2D);
                        var distance2 = Space2D.Distance(lineEquationResults.Result2, realVector2D);
                        //Debug.Log($"x1: {distance1}, x2: {distance2}");
                        bool flag = distance2 > distance1;
                        vector3d = flag ? lineEquationResults.Result1 : lineEquationResults.Result2;
                    }

                    var v3 = new Vector3(vector3d.x, pos.y, vector3d.y);
                    var di = toolBase.IsOkayToCutVertex(v3);
                    if (!di.IsZeroed())
                    {
                        //continue;
                        if (di.X < 0)
                        {
                            v3.x = toolBase.MinX;
                        }
                        else if (di.X > 0)
                        {
                            v3.x = toolBase.MaxX;
                        }

                        if (di.Y < 0)
                        {
                            v3.y = toolBase.MinY;
                        }
                        else if (di.Y > 0)
                        {
                            v3.y = toolBase.MaxY;
                        }
                        
                        if (di.Z < 0)
                        {
                            v3.z = toolBase.MinZ;
                        }
                        else if(di.Z > 0)
                        {
                            v3.z = toolBase.MaxZ;
                        }
                        //continue;
                    }
                    VertexTracker[i] += 1;
                    var actualVector = tr.InverseTransformPoint(v3);
                    toolBase.Colors[i] = toolBase.ToolConfig.ToolColor;
                    vertices[i] = actualVector;
                    verticesCut++;
                }
            }
            var wp = toolBase.Workpiece;
            wp.UpdateVertices(vertices.GetInternalArray());
            //mesh.colors = tool.Colors.GetInternalArray();
            //mesh.triangles = tool.Triangles.GetInternalArray();
            //mesh.GenerateVertexHashmap((tool as MillTool3D).increment);
            stopwatch.Stop();
            return new CutResult(stopwatch.Elapsed.TotalMilliseconds, verticesCut);*/
            return default;
        }

        [CustomMethod(nameof(TraverseFinal))]
        public static async Task TraverseFinal(this ToolBase toolBase, Vector3[] points, bool draw, bool logStats)
        {
            var traverseState = Globals.MethodManager.Get("Traverse");
            switch (traverseState.Index)
            {
                case -1:
                {
                    await toolBase.TraverseForceBased(points, draw, logStats);
                    break;
                }

                case 0:
                {
                    await toolBase.Traverse(points, draw, logStats);
                    break;
                }

                default:
                {
                    if (!CachedMethods.ContainsKey("Traverse"))
                    {
                        throw new MissingMethodException("This method has not been re-implemented yet.");
                    }

                    Task awaitableTask = CachedMethods["Traverse"].Invoke(null, new object[]{toolBase, points, draw, logStats}).CastInto<Task>();
                    await awaitableTask;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Vector3"/>[].
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="points">The tool's current path.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        [CustomMethod(nameof(Traverse))]
        public static async Task Traverse(this ToolBase toolBase, Vector3[] points, bool draw, bool logStats = true)
        {
            /*var machine = MachineBase.CurrentMachine;
            var toolValues = toolBase.Values;
            var throwIfCutIsDetected = machine.Runner.CurrentContext.GetType() == typeof(G00);
            double averageTimeForCut = 0;
            long totalCut = 0;
            toolBase.SetupTranslation(points);
            var currentPosition = toolBase.Values.Position;
            var last = points.Last();
            Dictionary<int, bool> dictVector = Enumerable.Range(0, toolBase.Vertices.Count).ToDictionary(k => k, _ => false);
            for (int i = 0; i < VertexTracker.Count; i++)
            {
                VertexTracker[i] = 0;
            }

            double totalDistance = Vector3.Distance(currentPosition, points[0]);
            
            for (int i = 1; i < points.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(points[i], points[i + 1]);
            }
            foreach (var point in points)
            {
                if (machine.StateControl.State == MachineState.Resetting)
                {
                    return;
                }

                var distance = Vector3.Distance(point, currentPosition);
                float msToTraverseReal;
                if (machine.SimControl.Unit == UnitType.Imperial)
                {
                    msToTraverseReal = (distance * 60_000 * 25.4f) / (machine.SpindleControl.FeedRate);
                }
                else
                {
                    msToTraverseReal = (distance * 60_000) / machine.SpindleControl.FeedRate;
                }
                var totalTime = TimeSpan.FromMilliseconds(msToTraverseReal);
                UI_3D.Instance.Time.Time += totalTime;
                draw.DrawTranslation(currentPosition, point);
                toolBase.Position = point;
                var cr = toolBase.CheckPositionForCut(dictVector, Direction.FromVectors(currentPosition, point), throwIfCutIsDetected);
                averageTimeForCut = cr.TotalTime / cr.TotalVerticesCut;
                totalCut = cr.TotalVerticesCut;
                currentPosition = point;
                if (cr.Threw)
                {
                    throw new RapidFeedCollisionException(machine.Runner.CurrentContext);
                }
                await FinishCurrentMove(cr, totalDistance);
                if (toolValues.TokenSource.IsCancellationRequested)
                {
                    Globals.Console.Push($"Cancelled task - {machine.Runner.CurrentContext.Id}!");
                    break;
                }

                if (currentPosition == last)
                {
                    break;
                }
            }
            if (toolValues.ExactStopCheck)
            {
                //await toolBase.InvokeOnConsumeStopCheck();
            }
            if (logStats)
            {
                PyroConsoleView.PushTextStatic("Traverse finished!", $"Total vertices cut: {totalCut} ({(((double) totalCut / toolBase.Vertices.Count) * 100).Round().ToString(CultureInfo.InvariantCulture)}%)",
                                               $"Average time spent cutting: {averageTimeForCut.Round().ToString(CultureInfo.InvariantCulture)}ms");
            }
            */
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Line3D"/>.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="line">The tool's current path.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ToolBase toolBase, Line3D line, bool draw, bool logStats = true)
        {
            await toolBase.TraverseFinal(line.ToVector3s(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by either a <see cref="Circle"/>, <see cref="Circle3D"/> or <see cref="Arc3D"/>.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="arc">The tool's current path.</param>
        /// <param name="reverse">Whether to reverse the direction of movement for the <see cref="I3DShape"/>.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ToolBase toolBase, Arc3D arc, bool draw, bool logStats = true)
        {
            await toolBase.TraverseFinal(arc.Points.ToArray(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by either a <see cref="Circle"/>, <see cref="Circle3D"/> or <see cref="Arc3D"/>.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="circleCenter">The circle's center.</param>
        /// <param name="circleRadius">The circle's radius.</param>
        /// <param name="reverse">Whether to reverse the direction of movement for the <see cref="Circle3D"/>.</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ToolBase toolBase, Vector3 circleCenter, float circleRadius, bool reverse, bool draw, bool logStats = true)
        {
            var circle3d = new Circle3D(circleRadius, toolBase.Position.y);
            if (reverse)
            {
                circle3d.Reverse();
            }
            circle3d.Shift(circleCenter.ToVector3D());
            
            await toolBase.TraverseFinal(circle3d.ToVector3s(), draw, logStats);
        }
        /// <summary>
        /// Attempts to traverse the along the path defined by a <see cref="Vector3"/> destination, later converted to a <see cref="Line3D"/>.
        /// </summary>
        /// <param name="toolBase">The tool.</param>
        /// <param name="destination">The tool's current destination.</param>
        /// <param name="smoothness">The total amount of points the tool is to traverse through.[overriden]</param>
        /// <param name="draw">Whether to draw the path or not.</param>
        /// <param name="logStats">Whether to log the (averageTimeForCut) and (totalCut).</param>
        public static async Task Traverse(this ToolBase toolBase, Vector3 destination, LineTranslationSmoothness smoothness, bool draw, bool logStats = true)
        {
            var p1 = toolBase.Position.ToVector3D();
            var p2 = destination.ToVector3D();
            var dist = Space3D.Distance(p1, p2);
            if (dist == 0)
            {
                return;
            }
            var line = new Line3D(toolBase.Position.ToVector3D(), destination.ToVector3D(), dist.Mutate(d =>
            {
                if (d < 5)
                {
                    return 10;
                }
                else if (d < 50)
                {
                    return 50;
                }

                return (int) d;
            }));
            await toolBase.Traverse(line, draw, logStats);
        }
        internal static async Task FinishCurrentMove(CutResult cutResult, double totalDistance)
        {
            var values = Globals.Tool.Values;
            var feed = MachineBase.CurrentMachine.SpindleControl.FeedRate;
            var time = totalDistance / (feed * 60_000);
            var remainder = TimeSpan.FromMilliseconds(time) - TimeSpan.FromMilliseconds(cutResult.TotalTime);
            if (remainder > TimeSpan.Zero)
            {
                await Task.Delay(remainder);
                Globals.Console.Push($"Waiting for remainer: {remainder.ToString()}!");
            }
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