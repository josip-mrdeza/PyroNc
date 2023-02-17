using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public static class Sim2D
{
    public static void Traverse2D(this ToolBase toolBase, Vector3 destination, LineTranslationSmoothness smoothness)
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
        toolBase.Traverse2D(line);
    }

    public static void Traverse2D(this ToolBase toolBase, Line3D line)
    {
        toolBase.TraverseFinal2D(line.ToVector3s());
    }

    public static void TraverseFinal2D(this ToolBase toolBase, Vector3[] points)
    {
        /*toolBase.SetupTranslation(points);
        var arr = new Vector3[toolBase.LineRenderer.positionCount];
        toolBase.LineRenderer.GetPositions(arr);
        var nextArr = arr.Concat(points);
        toolBase.LineRenderer.SetPositions(nextArr.ToArray());*/
    }
}