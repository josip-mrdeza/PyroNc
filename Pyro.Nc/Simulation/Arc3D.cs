using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Pyro.Math;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class Arc3D
{
    public IEnumerable<Vector3> Points { get; }
    public float Radius { get; }

    public Arc3D(float radius, Vector3 center, Vector3 start, Vector3 end, bool isReverse = false)
    {
        Radius = radius;
        Points = GenerateArcPoints(radius, center, start, end, isReverse);
    }

    private IEnumerable<Vector3> GenerateArcPoints(float radius, Vector3 center, Vector3 start, Vector3 end, bool isReverse = false)
    {
        //Get the points in the unit circle (circle with a radius of 1f).
        var centerAsVc2 = new Vector2(center.x, center.z);
        var startUnitCircle = (new Vector2(start.x, start.z) - centerAsVc2) / radius;
        var endUnitCircle = (new Vector2(end.x, end.z) - centerAsVc2) / radius;
        if (endUnitCircle.x > 1)
        {
            endUnitCircle.x = 1;
        }
        else if (endUnitCircle.x < -1)
        {
            endUnitCircle.x = -1;
        }
        if (endUnitCircle.y > 1)
        {
            endUnitCircle.y = 1;
        }
        else if (endUnitCircle.y < -1)
        {
            endUnitCircle.y = -1;
        }
        if (startUnitCircle.x > 1)
        {
            startUnitCircle.x = 1;
        }
        else if (startUnitCircle.x < -1)
        {
            startUnitCircle.x = -1;
        }
        if (startUnitCircle.y > 1)
        {
            startUnitCircle.y = 1;
        }
        else if (startUnitCircle.y < -1)
        {
            startUnitCircle.y = -1;
        }
        //Get the equivalent degree on the unit circle.
        var startDegree = startUnitCircle.x.ArcCos().ToDegreeFromRadian();
        var endDegree = endUnitCircle.x.ArcCos().ToDegreeFromRadian();

        if (startUnitCircle.y < 0)
        {
            startDegree -= 2 * startDegree;
        }

        if (endUnitCircle.y < 0)
        {
            endDegree -= 2 * endDegree;
        }

        if (isReverse)
        {
            //(startDegree, endDegree) = (endDegree, startDegree);
        }
        
        var averageDepth = Operations.Average(start.y, end.y);
        if (System.Math.Abs(startDegree - endDegree) < 0.001f)
        {
            startDegree += 360f;
        }

        if (startDegree < 0)
        {
            startDegree += 360f;
        }

        if (endDegree < 0)
        {
            endDegree += 360f;
        }
        //return points with a circle of radius R.
        if (!isReverse)
        {
            for (float i = startDegree; i >= endDegree; i--)
            {
                yield return new Vector3((i.Cos() * radius) + center.x, averageDepth, (i.Sin() * radius) + center.z);
            }
        }
        else
        {
            for (float i = startDegree; i <= endDegree; i++)
            {
                yield return new Vector3((i.Cos() * radius) + center.x, averageDepth, (i.Sin() * radius) + center.z);
            }
        }
    }
}