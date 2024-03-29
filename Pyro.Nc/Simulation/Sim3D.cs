using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Algos;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Path = Pyro.Nc.Pathing.Path;

namespace Pyro.Nc.Simulation
{
    public static class Sim3D
    {
        [StoreAsJson]
        public static bool RealtimeCutting { get; set; }
        [StoreAsJson]
        public static float WaitStep { get; set; }
        [StoreAsJson]
        public static bool KeepTraceLines { get; set; }

        [StoreAsJson]
        public static CutType CuttingType { get; set; } = CutType.LineHash;

        private static TimeSpan WaitStep_Time => TimeSpan.FromMilliseconds(WaitStep);
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
        public static Direction IsOkayToCutVertexDirection(this Vector3 vertex)
        {
            var d = new Direction();
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            var min = workpiece.MinValues;
            var max = workpiece.MaxValues;
            //vertex = tool.Cube.transform.TransformVector(vertex);
            if (vertex.x < min.x)
            {
                d.X = vertex.x;
            }
            else if (vertex.x > max.x)
            {
                d.X = vertex.x;
            }
            //
            if (vertex.y < min.y)
            {
                d.Y = vertex.y;
            }
            else if (vertex.y > max.y)
            {
                d.Y = vertex.y;
            }
            //
            if (vertex.z < min.z)
            {
                d.Z = vertex.z;
            }
            else if (vertex.z > max.z)
            {
                d.Z = vertex.z;
            }

            return d;
        }

        public static bool IsOkayToCutVertex(this Vector3 vertex)
        {
            var d = new Direction();
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            var min = workpiece.MinValues;
            var max = workpiece.MaxValues;
            var safetyMargin = 0.2f;
            if (vertex.x < min.x - safetyMargin)
            {
                d.X = -vertex.x;
            }
            else if (vertex.x > max.x + safetyMargin)
            {
                d.X = vertex.x;
            }
            //
            if (vertex.y < min.y - safetyMargin)
            {
                d.Y = -vertex.y;
            }
            else if (vertex.y > max.y + safetyMargin)
            {
                d.Y = vertex.y;
            }
            //
            if (vertex.z < min.z - safetyMargin)
            {
                d.Z = -vertex.z;
            }
            else if (vertex.z > max.z + safetyMargin)
            {
                d.Z = vertex.z;
            }

            return d.X == 0 && d.Y == 0 && d.Z == 0;
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
            var isCutting = MachineBase.CurrentMachine.SimControl.IsInCuttingMode;
            toolBase.Renderer.positionCount = points.Length;
            Dictionary<Vector3, List<int>> dict = null;
            if (CuttingType == CutType.LineHash || CuttingType == CutType.LineHashAdditive)    
            {
                //dict = await Task.Run(async () => await ToolBase.CompileLineHashCut(points));
                dict = await Task.Run(async () =>
                {
                    var algo = (CompiledLineHashAlgorithm) CustomAssemblyManager.Algorithms[(int)CuttingType];
                    await algo.PrefixAsync(points);
                    return algo.CompiledLine;
                });
            }
            var boxHash = MachineBase.CurrentMachine.Workpiece.VertexBoxHash;
            var maxMapping = new Dictionary<int, int>();
            var boxes = GenerateVertexBoxHashRequirements(isCutting, boxHash, WorkpieceControl.Step);
            if (KeepTraceLines)
            {
                Vector3[] arr = new Vector3[toolBase.Renderer.positionCount];
                toolBase.Renderer.GetPositions(arr);
                List<Vector3> vs = arr.ToList();
                vs.AddRange(points);     
                toolBase.Renderer.positionCount = vs.Count;
                var arr2 = vs.ToArray();
                toolBase.Renderer.SetPositions(arr2);
            }
            else
            {
                toolBase.Renderer.SetPositions(points);
            }

            var instance = UI_3D.Instance;
            for (var i = 0; i < points.Length; i++)
            {
                if (MachineBase.CurrentMachine.StateControl.IsResetting)
                {
                    return;
                }
                var point = points[i];
                var diff = Vector3.Distance(point, toolBase.Position);
                toolBase.Position = point;
                if (isCutting)
                {
                    if (dict.TryGetValue(point, out var list))
                    {
                        var wp = MachineBase.CurrentMachine.Workpiece;
                        var tr = wp.transform;
                        var verts = wp.Vertices;
                        foreach (var index in list)
                        {
                            var v = verts[index];
                            if (MachineBase.CurrentMachine.ToolControl.SelectedTool.IsCollidingToolShankAt(
                                    tr.TransformPoint(v)))
                            {
                                throw new CollisionWithToolShankException();
                            }
                        }
                    }

                    IMillAlgorithm algo = CustomAssemblyManager.Algorithms[(int) CuttingType];
                    await algo.MillAsync(toolBase, WorkpieceControl.Instance);
                    var feedPerMin = (float) MachineBase.CurrentMachine.SpindleControl.FeedRate;
                    var minutes = diff / feedPerMin;
                    try
                    {
                        instance.IncrementTimeDisplay(TimeSpan.FromMinutes(minutes));
                    }
                    catch
                    {
                        //ignored
                    }
                    MachineBase.CurrentMachine.Workpiece.UpdateVertices();
                }
                else
                {
                    if (!dict.TryGetValue(point, out var list))
                    {
                        if (WaitStep != 0)
                        {
                            var feedPerMin = (float) MachineBase.CurrentMachine.SpindleControl.FeedRate;
                            var minutes = diff / feedPerMin;
                            try
                            {
                                instance.IncrementTimeDisplay(TimeSpan.FromMinutes(minutes));
                            }
                            catch
                            {
                                //ignored
                            }
                            await FinishMove(true, 0);
                        }
                        continue;
                    }
                    var wp = MachineBase.CurrentMachine.Workpiece;
                    var tr = wp.transform;
                    var verts = wp.Vertices;
                    foreach (var index in list)
                    {
                        var v = verts[index];
                        if (MachineBase.CurrentMachine.ToolControl.SelectedTool.IsCollidingWithWorkpieceAt(tr.TransformPoint(v)))
                        {
                            throw new RapidFeedCollisionException(point);
                        }
                    }

                    continue;
                }
                if (WaitStep != 0)
                {
                    await FinishMove(false, diff);
                }
            }
            
            if (CuttingType == CutType.VertexBoxHash)
            {
                MachineBase.CurrentMachine.Workpiece.GenerateVertexBoxHashes(WorkpieceControl.Step,
                                                                             HashmapGenerationReason.UpdatedVertices);
            }
            else
            {
                //Remesh();
            }
        }

        private static KeyValuePair<Vector3Range, List<Algorithms.VertexMap>>[] GenerateVertexBoxHashRequirements(bool isCutting,
            Dictionary<Vector3Range, List<Algorithms.VertexMap>> boxHash, float radius)
        {
            KeyValuePair<Vector3Range, List<Algorithms.VertexMap>>[] boxes = null;
            if (isCutting && CuttingType == CutType.VertexBoxHash)
            {
                boxes = boxHash.Where(x => x.Key.End.y >= radius).ToArray();
                var sum = boxes.Sum(x => x.Value.Count);
                var eff = MachineBase.CurrentMachine.Workpiece.Vertices.Count / (double)sum * 100;
                Globals.Console.Push($"Cutting with {sum.ToString()} points!\n" +
                                     $"    --Efficiency: {eff.Round().ToString(CultureInfo.InvariantCulture)}% ({(eff / 100).Round().ToString(CultureInfo.InvariantCulture)}x less iterations)");
            }

            return boxes;
        }

        public static async Task FinishMove(bool skipYield = false, float distTravelled = 0f)
        {
            if (RealtimeCutting && distTravelled > 0)
            {
                var feedPerSec = (float) MachineBase.CurrentMachine.SpindleControl.FeedRate / 60;
                await Task.Delay(TimeSpan.FromMilliseconds(distTravelled / feedPerSec));
                if (!skipYield)
                {
                    await Task.Yield();
                }  
            }
            else
            {
                await Task.Delay(WaitStep_Time);
                if (!skipYield)
                {
                    await Task.Yield();
                }
            }
        }
        public static void Remesh()
        {
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            var verts = workpiece.Vertices;
            List<int> triangles = new List<int>();
            for (int i = 0;; i++)
            {
                var fv = i * 2;
                var lv = i * 2 + 3;
                var maxV = verts.Count - 1;
                
                if (fv <= maxV && lv <= maxV)
                {
                    triangles.Add(i * 2);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 2);
                
                    triangles.Add(i * 2 + 2);
                    triangles.Add(i * 2 + 1);
                    triangles.Add(i * 2 + 3);
                }

                if (i > 100_000_000)
                {
                    break;
                }
            }
            //workpiece.Triangles = triangles.ToArray();
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
            var arr = arc.Points.ToArray();
            Globals.Console.Push($"[Traverse(ARC3D)]: Arc with R={arc.Radius} contains {arr.Length} points.");
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