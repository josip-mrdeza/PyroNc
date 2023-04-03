using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Nc.UI.Debug;
using UnityEngine;

namespace Pyro.Nc.Simulation.Algos;

public class CompiledLineHashAlgorithm : MachineComponent, IMillAlgorithm
{
    public virtual string Name { get; set; } = "Compiled Line Hash";
    public virtual int Id => AlgorithmId;
    public const int AlgorithmId = (int)CutType.LineHash;
    public Dictionary<Vector3, List<int>> CompiledLine { get; private set; }
    public Dictionary<Vector3, List<int>> SurfaceCompiledLine { get; private set; }
    public void Prefix(Vector3[] toolPathPoints)
    {
    }

    public async Task PrefixAsync(Vector3[] toolPathPoints)
    {
        CompiledLine = await CompileLine(toolPathPoints);
        SurfaceCompiledLine = await CompileSurfaceLine(toolPathPoints);
    }

    public virtual void Postfix(int index, Transform tr, Vector3 v, Color color)
    {
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var colors = control.Colors;
        vertices[index] = tr.InverseTransformPoint(v);
        colors[index] = color * 3f;
    }
    public void Mill(ToolBase tool, WorkpieceControl workpiece)
    {
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var pos = tool.Position;
        var tr = control.transform;
        if (!CompiledLine.TryGetValue(pos, out var list))
        {
            return;
        }

        var color = tool.ToolConfig.GetColor();
        var rad = machine.ToolControl.SelectedTool.Values.Radius;
        bool isg2 = machine.Runner.CurrentContext is G02 or Cycle;
        foreach (var i in list)
        {
            var v = tr.TransformPoint(vertices[i]);
            if (v.y < pos.y)
            {
                continue;
            }
            if (v.y > pos.y + tool.ToolConfig.VerticalMargin)
            {
                throw new CollisionWithToolShankException();
            }

            if (isg2)
            {
                v.ShowPoint(15, Color.red);
                DoCircleFix(pos, ref v, rad, pos.y, machine);
                v.ShowPoint(15, Color.green);
            }
            else
            {
                v.y = pos.y;
            }

            Postfix(i, tr, v, color);
        }
        if (isg2 && SurfaceCompiledLine.TryGetValue(pos, out var l2))
        {
            foreach (var i in l2)
            {
                var v = tr.TransformPoint(vertices[i]);
                v.ShowPoint(15, Color.red);
                DoCircleFix(pos, ref v, rad, v.y, machine);
                v.ShowPoint(15, Color.green);
                Postfix(i, tr, v, color);
            }
        }
    }

    private void DoCircleFix(Vector3 pos, ref Vector3 v, float rad, float ypos, MachineBase machine)
    {
        var line = new Line2D(new Vector2D(pos.x, pos.z), new Vector2D(v.x, v.z));
        var results = line.FindCircleEndPoint(rad, pos.x, pos.z);
        if (results.ResultOfDiscriminant == Line2D.LineEquationResults.DiscriminantResult.Imaginary)
        {
            v.y = ypos;        
        }
        else
        {
            var tempRes = (results.ResultOfDiscriminant switch
            {
                Line2D.LineEquationResults.DiscriminantResult.One  => results.Result1,
                Line2D.LineEquationResults.DiscriminantResult.Dual => GetCorrectResult(results, v),
                _                                                  => throw new ArgumentOutOfRangeException()
            });
            v = new Vector3(tempRes.x, ypos, tempRes.y);
            var dir = v.IsOkayToCutVertexDirection();

            var min = machine.Workpiece.MinValues;
            var max = machine.Workpiece.MaxValues;
            if (dir.X < 0)
            {
                v.x = min.x;

                //v.ShowPoint(25, Color.blue);
            }

            if (dir.X > 0)
            {
                v.x = max.x;

                //v.ShowPoint(25, Color.red);
            }

            if (dir.Y < 0)
            {
                v.y = min.y;
            }

            if (dir.Y > 0)
            {
                v.y = max.y;
            }

            if (dir.Z < 0)
            {
                v.z = min.z;
            }

            if (dir.Z > 0)
            {
                v.z = max.z;
            }
        }
    }

    protected Vector2D GetCorrectResult(Line2D.LineEquationResults results, Vector3 v)
    {
        var r1 = new Vector2(results.Result1.x, results.Result1.y);
        var r2 = new Vector2(results.Result2.x, results.Result2.y);

        var p = new Vector2(v.x, v.z);
        var d1 = Vector2.Distance(r1, p);
        var d2 = Vector2.Distance(r2, p);

        return d1 > d2 ? results.Result2 : results.Result1;
    }
    public Task MillAsync(ToolBase tool, WorkpieceControl workpiece)
    {
        Mill(tool, workpiece);
        return Task.CompletedTask;
    }

    public async Task<Dictionary<Vector3, List<int>>> CompileSurfaceLine(Vector3[] toolPathPoints)
    {
        Stopwatch s = Stopwatch.StartNew();
        Dictionary<Vector3, List<int>> vecToListHash = new Dictionary<Vector3, List<int>>();
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var toolConfig = Globals.Tool.ToolConfig;
        var radius = toolConfig.Radius;
        var transform = await machine.Queue.Run(GetTransformMainThread, control);
        var transformedVertices = await machine.Queue.Run(GetTransformedVerticesMainThread,
                                                          vertices, transform);
        var count = transformedVertices.Count;
        var sum = 0;
        for (int transformedVIndex = 0; transformedVIndex < count; transformedVIndex++)
        {
            var transformedVertex = transformedVertices[transformedVIndex];
            foreach (var pathPoint in toolPathPoints)
            {
                if (!IsInRangeToFix(transformedVertex, pathPoint, radius, toolConfig.VerticalMargin))
                {
                    continue;
                }

                bool hashHasList = vecToListHash.TryGetValue(pathPoint, out var list);
                if (!hashHasList)
                {
                    list = new List<int>();
                    vecToListHash.Add(pathPoint, list);
                }
                list.Add(transformedVIndex);
                sum++;
            }
        }
        s.Stop();
        Globals.Tool.Push($"[ToolBase] - Compiled surface vertices in '{s.Elapsed.TotalMilliseconds.Round().ToString(CultureInfo.InvariantCulture)}'ms\n" +
                          $"    -Total buckets: '{vecToListHash.Count.ToString(CultureInfo.InvariantCulture)}',\n" +
                          $"    -Total vertex sum: '{sum}',\n" +
                          $"    -Execution complete on thread: '{Thread.CurrentThread.ManagedThreadId.ToString()}'.");
        return vecToListHash;
    }

    public async Task<Dictionary<Vector3, List<int>>> CompileLine(Vector3[] toolPathPoints)
    {
        Stopwatch s = Stopwatch.StartNew();
        Dictionary<Vector3, List<int>> vecToListHash = new Dictionary<Vector3, List<int>>();
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var toolConfig = Globals.Tool.ToolConfig;
        var radius = toolConfig.Radius;
        var transform = await machine.Queue.Run(GetTransformMainThread, control);
        var transformedVertices = await machine.Queue.Run(GetTransformedVerticesMainThread,
                                                          vertices, transform);
        var count = transformedVertices.Count;
        var sum = 0;
        for (int transformedVIndex = 0; transformedVIndex < count; transformedVIndex++)
        {
            var transformedVertex = transformedVertices[transformedVIndex];
            foreach (var pathPoint in toolPathPoints)
            {
                if (!IsInRangeToCut(transformedVertex, pathPoint, radius, toolConfig.VerticalMargin))
                {
                    continue;
                }

                bool hashHasList = vecToListHash.TryGetValue(pathPoint, out var list);
                if (!hashHasList)
                {
                    list = new List<int>();
                    vecToListHash.Add(pathPoint, list);
                }
                list.Add(transformedVIndex);
                sum++;
            }
        }
        s.Stop();
        Globals.Tool.Push($"[ToolBase] - Compiled vertices in '{s.Elapsed.TotalMilliseconds.Round().ToString(CultureInfo.InvariantCulture)}'ms\n" +
                          $"    -Total buckets: '{vecToListHash.Count.ToString(CultureInfo.InvariantCulture)}',\n" +
                          $"    -Total vertex sum: '{sum}',\n" +
                          $"    -Execution complete on thread: '{Thread.CurrentThread.ManagedThreadId.ToString()}'.");
        return vecToListHash;
    }

    private bool IsInRangeToCut(Vector3 transformedVertex, Vector3 pathPoint, float radius, float verticalMargin)
    {
        var workpiecePoint2d = new Vector2(transformedVertex.x, transformedVertex.z);
        var pathPoint2d = new Vector2(pathPoint.x, pathPoint.z);
        var horizontalDistance = Vector2.Distance(workpiecePoint2d, pathPoint2d);
        if (horizontalDistance > radius) //point is farther than 105% radius.
        {
            return false;
        }

        var verticalDistance = (pathPoint.y - transformedVertex.y).Abs();
        if (verticalDistance > verticalMargin)
        {
            return false;
        }

        return true;
    }
    
    private bool IsInRangeToFix(Vector3 transformedVertex, Vector3 pathPoint, float radius, float verticalMargin)
    {
        var workpiecePoint2d = new Vector2(transformedVertex.x, transformedVertex.z);
        var pathPoint2d = new Vector2(pathPoint.x, pathPoint.z);
        var horizontalDistance = Vector2.Distance(workpiecePoint2d, pathPoint2d);
        if (horizontalDistance > radius + 0.3f * radius) //point is farther than 105% radius.
        {
            return false;
        }

        if (pathPoint.y > transformedVertex.y)
        {
            return false;
        }
        return true;
    }
    
    private static List<Vector3> GetTransformedVerticesMainThread(List<Vector3> original, Transform tr)
    {
        List<Vector3> verts = original.ToList();
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = tr.TransformPoint(verts[i]);
        }

        return verts;
    }

    private static Transform GetTransformMainThread(WorkpieceControl control)
    {
        return control.transform;
    }
}