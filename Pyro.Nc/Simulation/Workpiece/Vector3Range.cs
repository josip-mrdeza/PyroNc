using System;
using System.Linq;
using System.Text.Json.Serialization;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Simulation.Machines;
using UnityEngine;

namespace Pyro.Nc.Simulation.Workpiece;

[Serializable]
public readonly struct Vector3Range
{
    public readonly Vector3 Start;
    public readonly Vector3 End;

    public Vector3Range(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public bool Fits(Vector3 v, float margin = 0.005f)
    {
        var isOk = true;
        
        if (v.x > Start.x && v.x < End.x)
        {
            isOk = false;
        }

        if (v.y > Start.y && v.y < End.y)
        {
            isOk = false;
        }

        if (v.z > Start.z && v.z < End.z)
        {
            isOk = false;
        }
        return isOk;
    }

    public bool FitsXZ(Vector3 v)
    {
        var isOk = true;
        
        if (v.x > Start.x && v.x < End.x)
        {
            isOk = false;
        }

        if (v.z > Start.z && v.z < End.z)
        {
            isOk = false;
        }
        return isOk;
    }

    public bool FitsArc(Vector3 center, float circleRadius)
    {
        var up = center + new Vector3(circleRadius, 0, 0);
        Arc3D arc = new Arc3D(circleRadius, center, up, up);
        var arr = arc.Points.ToArray();
        if (Fits(center))
        {
            return true;
        }
        foreach (var point in arr)
        {
            if (Fits(point))
            {
                return true;
            }
        }

        return false;
    }
}