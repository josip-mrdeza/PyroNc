using System;
using System.Collections.Generic;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class Sim2DView : View
{
    public LineRenderer Renderer;
    public static Sim2DView Instance;
    public List<Vector3> points;
    public override void Initialize()
    {
        base.Initialize();
        Instance = this;
        Renderer = gameObject.AddComponent<LineRenderer>();
        points = new List<Vector3>();
    }

    public void Clear()
    {
        Renderer.positionCount = 0;
        Renderer.SetPositions(Array.Empty<Vector3>());
    }

    public void Set(Vector3[] arr)
    {
        Renderer.positionCount = arr.Length;
        Renderer.SetPositions(arr);
    }

    public void AddPoints(Vector3[] arr)
    {
        points.AddRange(arr);
        Renderer.positionCount = points.Count;
        Renderer.SetPositions(points.ToArray());
    }
}