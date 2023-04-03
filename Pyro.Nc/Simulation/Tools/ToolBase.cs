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

    public bool IsCollidingToolShankAt(Vector3 v)
    {
        var pos = Position;
        var hor = Vector2.Distance(new Vector2(v.x, v.z), new Vector2(pos.x, pos.z));
        var vert = v.y >= pos.y + ToolConfig.VerticalMargin;
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