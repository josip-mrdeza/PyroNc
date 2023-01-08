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
        
        //Get the equivalent degree on the unit circle.
        var startDegree = startUnitCircle.x.ArcCos().ToDegreeFromRadian();
        var endDegree = endUnitCircle.x.ArcCos().ToDegreeFromRadian();
        
        var averageDepth = Operations.Average(start.y, end.y);
        if (System.Math.Abs(startDegree - endDegree) < 0.001f)
        {
            startDegree += 360f;
        }
        Globals.Console.Push($"Arc - " +
                             $"{startDegree.Round().ToString(CultureInfo.InvariantCulture)}," +
                             $" {endDegree.Round().ToString(CultureInfo.InvariantCulture)}");
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
            for (float i = endDegree; i <= startDegree; i++)
            {
                yield return new Vector3((i.Cos() * radius) + center.x, averageDepth,(i.Sin() * radius) + center.z);
            }
        }
    }
}