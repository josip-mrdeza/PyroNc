using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation.Tools;

public class ToolBase : InitializerRoot
{
    public ToolValues Values { get; protected set; }

    public ToolConfiguration ToolConfig
    {
        get
        {
            if (_config is null)
            {
                throw new ToolNotDefinedException();
            }

            return _config;
        }
        set
        {
            _config = value;
        }
    }
    public Rigidbody Body { get; private set; }
    public Transform Transform => _transform;
    public Transform VTempTransform => _vtTransform;

    public Vector3 CutterCenterPosition
    {
        get => _vtTransform.position;
        set => _vtTransform.position = value;
    }

    public Vector3 Position
    {
        get => _transform.position;
        set
        {
            PreviousPosition = Position;
            _transform.position = value;
            MachineBase.CurrentMachine?.EventSystem.PositionChanged();
        }
    }
    public Vector3 PreviousPosition { get; set; }
    public LineRenderer Renderer;
    private Transform _transform;
    private Transform _vtTransform;
    private ToolConfiguration _config;

    public override void Initialize()
    {
        Body = GetComponent<Rigidbody>();
        _transform = gameObject.transform;
        _vtTransform = gameObject.transform.GetChild(1);
        Values = new ToolValues(this);
        Position = Globals.ReferencePointHandler.BeginPoint.Position;
        Renderer = GetComponent<LineRenderer>();
        Globals.Tool = this;
    }

    public bool IsCollidingWithWorkpieceAt(Vector3 v)
    {
        var pos = Position;
        var hor = Vector2.Distance(new Vector2(v.x, v.z), new Vector2(pos.x, pos.z));
        var vert = v.y >= pos.y;
        return vert && hor < ToolConfig.Radius;
    }
    public void CutLegacy()
    {
        var control = MachineBase.CurrentMachine.Workpiece;
        var count = control.Vertices.Count;
        var tr = control.transform;
        for (int i = 0; i < count; i++)
        {
            var map = new Algorithms.VertexMap(tr.TransformPoint(control.Vertices[i]), i);
            Deform(map, default);
        }
    }
    public void Cut(KeyValuePair<Vector3Range, List<Algorithms.VertexMap>>[] kvps, Dictionary<int, int> maxMap)
    {
        for (int i = 0; i < kvps.Length; i++)
        {
            var kvp = kvps[i];
            var range = kvp.Key;
            var maps = kvp.Value;
            for (var index = 0; index < maps.Count; index++)
            {
                var map = maps[index];
                if (maxMap.TryGetValue(map.Index, out var value))
                {
                    if (value > 2)
                    {
                        continue;
                    }
                    else
                    {
                        maxMap.Add(map.Index, 1);
                    }
                }
                Deform(map, range);
            }
        }
    }

    public void Deform(Algorithms.VertexMap map, Vector3Range range)
    {
        var currentMachine = MachineBase.CurrentMachine;
        var workpiece = currentMachine.Workpiece;
        var toolConfig = currentMachine.ToolControl.SelectedTool.ToolConfig;
        var radius = toolConfig.Radius;
        var pos = Position;
        var v = map.Vertex;

        var verticalDistance = Space3D.Distance(v.y, CutterCenterPosition.y);
        if (verticalDistance > toolConfig.VerticalMargin)
        {
            return;
        }

        var dist = Vector3.Distance(v, pos);
        if (dist > radius)
        {
            return;
        }

        if (CreateRadiusCircle(v, pos, radius, out var v2dResult))
        {
            return;
        }
        
        var v3dPrep = new Vector3(v2dResult.x, pos.y, v2dResult.y);
        if (v3dPrep.y > v.y)
        {
            v3dPrep.y = v.y;
        }
        var isOk = v3dPrep.IsOkayToCutVertex();
        if (!isOk)
        {
            //return;
        }

        //var v3dFinal = _transform.TransformPoint(v3dPrep);
        var v3dFinal = workpiece.transform.InverseTransformPoint(v3dPrep);
        var index = map.Index;
        workpiece.Vertices[index] = v3dFinal;
        workpiece.Colors[index] = toolConfig.GetColor(); 
    }

    public void LineHashCut(Dictionary<Vector3, List<int>> dictionary, Vector3 point)
    {
        var machine = MachineBase.CurrentMachine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var colors = control.Colors;
        var pos = Position;
        var tr = control.transform;
        if (!dictionary.TryGetValue(point, out var list))
        {
            return;
        }

        var color = ToolConfig.GetColor();
        for (var index = 0; index < list.Count; index++)
        {
            var i = list[index];
            var v = tr.TransformPoint(vertices[i]);
            if (v.y < pos.y)
            {
                continue;
            }
            else if (v.y > pos.y + ToolConfig.VerticalMargin)
            {
                throw new CollisionWithToolShankException();
            }

            if (false && !CreateRadiusCircle(v, pos, ToolConfig.Radius-0.3f, out var result))
            {
                var max = machine.Workpiece.MaxValues;
                if (result.x > max.x)
                {
                    result.x = max.x;
                }
                else if (result.x < 0)
                {
                    result.x = 0;
                }

                if (result.y > max.z)
                {
                    result.y = max.z;
                }
                else if (result.y < 0)
                {
                    result.y = 0;
                }

                v = new Vector3(result.x, pos.y, result.y);
            }
            else
            {
                v.y = pos.y;
            }

            vertices[i] = tr.InverseTransformPoint(v);
            colors[i] = color * 3f;
        }
    }

    public static async Task<Dictionary<Vector3, List<int>>> CompileLineHashCut(Vector3[] points)
    {
        Stopwatch s = Stopwatch.StartNew();
        var machine = MachineBase.CurrentMachine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var toolConfig = Globals.Tool.ToolConfig;
        var radius = toolConfig.Radius;
        Dictionary<Vector3, List<int>> vecToListHash = new Dictionary<Vector3, List<int>>();
        var transform = await machine.Queue.Run(GetTransformMainThread, control);
        var transformedVertices = await machine.Queue.Run(GetTransformedVerticesMainThread, vertices, transform);
        var count = transformedVertices.Count;
        for (var index = 0; index < count; index++)
        {
            var vertex = transformedVertices[index];
            foreach (var point in points)
            {
                var horizontalDistance = Vector2.Distance(new Vector2(vertex.x, vertex.z), new Vector2(point.x, point.z));
                if (horizontalDistance > radius)
                {
                    continue;
                }

                var verticalDistance = Vector2.Distance(new Vector2(0, vertex.y), new Vector2(0, point.y));
                if (verticalDistance > toolConfig.VerticalMargin)
                {
                    continue;
                }
                if (!vecToListHash.TryGetValue(point, out var list))
                {
                    list = new List<int>();
                    vecToListHash.Add(point, list);
                }
                list.Add(index); 
            }
        }
        var sum = vecToListHash.Sum(x => x.Value.Count).ToString();
        s.Stop();
        Globals.Tool.Push($"[ToolBase] - Compiled vertices in '{s.Elapsed.TotalMilliseconds.Round().ToString(CultureInfo.InvariantCulture)}'ms\n" +
                          $"    -Total buckets: '{vecToListHash.Count.ToString(CultureInfo.InvariantCulture)}',\n" +
                          $"    -Total vertex sum: '{sum}',\n" +
                          $"    -Execution complete on thread: '{Thread.CurrentThread.ManagedThreadId.ToString()}'.");
        return vecToListHash;
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

    private static bool CreateRadiusCircle(Vector3 vertex, Vector3 pos, float radius, out Vector2D v2dResult)
    {
        v2dResult = default;
        var v2d = new Vector2D(vertex.x, vertex.z);
        var pos2d = new Vector2D(pos.x, pos.z);
        Line2D line2d = new Line2D(v2d, pos2d);
        var leqr = line2d.FindCircleEndPoint(radius, pos2d.x, pos2d.y);
        if (leqr.ResultOfDiscriminant == Line2D.LineEquationResults.DiscriminantResult.Imaginary)
        {
            return true;
        }

        if (leqr.ResultOfDiscriminant == Line2D.LineEquationResults.DiscriminantResult.One)
        {
            v2dResult = leqr.Result1;
        }
        else
        {
            var distance1 = Space2D.Distance(leqr.Result1, v2d);
            var distance2 = Space2D.Distance(leqr.Result2, v2d);
            bool flag = distance2 > distance1;
            v2dResult = flag ? leqr.Result1 : leqr.Result2;
        }

        return false;
    }
}