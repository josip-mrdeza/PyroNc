using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public static class Sim2D
{
    public static void Traverse2D(this ITool tool, Vector3 destination, LineTranslationSmoothness smoothness)
    {
        var p1 = tool.Position.ToVector3D();
        var p2 = destination.ToVector3D();
        var dist = Space3D.Distance(p1, p2);
        if (dist == 0)
        {
            return;
        }
        var line = new Line3D(tool.Position.ToVector3D(), destination.ToVector3D(), dist.Mutate(d =>
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
        tool.Traverse2D(line);
    }

    public static void Traverse2D(this ITool tool, Line3D line)
    {
        tool.TraverseFinal2D(line.ToVector3s());
    }

    public static void TraverseFinal2D(this ITool tool, Vector3[] points)
    {
        tool.SetupTranslation(points);
        var arr = new Vector3[tool.LineRenderer.positionCount];
        tool.LineRenderer.GetPositions(arr);
        var nextArr = arr.Concat(points);
        tool.LineRenderer.SetPositions(nextArr.ToArray());
    }
}