using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Math;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public static class Algorithms
{
    public static Line3D CalculateParallelLineWithDistance(this Line3D line, float distance)
    {
        var startAlt = line.Start + new Vector3D(distance, 0, 0);
        var endAlt = line.End + new Vector3D(distance, 0, 0);
        var parallel = new Line3D(startAlt, endAlt, line.NumberOfPoints);

        return parallel;
    }

    public static bool IsInBox(this ValueTuple<Vector3, Vector3> vt, Vector3 vec)
    {
        bool isXOk = vt.Item1.x <= vec.x && vt.Item2.x >= vec.x;
        bool isYOk = vt.Item1.y <= vec.y && vt.Item2.y >= vec.y;
        bool isZOk = vt.Item1.z <= vec.z && vt.Item2.z >= vec.z;

        return isXOk && isYOk && isZOk;
    }
    public static Dictionary<ValueTuple<Vector3, Vector3>, List<VertexMap>> GenerateVertexHashmap(this Mesh mesh, float increment)
    {
        Dictionary<ValueTuple<Vector3, Vector3>, List<VertexMap >> dict = new();
        var tr = Globals.Workpiece.transform;
        var vertList = mesh.vertices.Select(y => tr.TransformPoint(y)).ToArray();
        return GenerateVertexHashmapCoreFunction(increment, vertList, dict);
    }

    public static Dictionary<ValueTuple<Vector3, Vector3>, List<VertexMap>> GenerateVertexHashmapLocalSpace(
        this Mesh mesh, float increment)
    {
        Dictionary<ValueTuple<Vector3, Vector3>, List<VertexMap>> dict = new();
        var vertList = mesh.vertices;
        return GenerateVertexHashmapCoreFunction(increment, vertList, dict);
    }

    private static Dictionary<(Vector3, Vector3), List<VertexMap>> GenerateVertexHashmapCoreFunction(float increment, Vector3[] vertList, Dictionary<(Vector3, Vector3), List<VertexMap>> dict)
    {
        var maxX = vertList.Max(x => x.x);
        var maxZ = vertList.Max(z => z.z);
        var maxY = vertList.Max(y => y.y);
        var xCount = (int)System.Math.Ceiling(maxX / increment);
        var zCount = (int)System.Math.Ceiling(maxZ / increment);
        var yCount = (int)System.Math.Ceiling(maxY / increment);
        for (var i = 0; i < vertList.Length; i++)
        {
            var vert = vertList[i];
            var vertMap = new VertexMap(vert, i);
            for (int x = 0; x < xCount; x++)
            {
                var xMin = x * increment;
                var xMax = (x + 1) * increment;
                bool isOk = vert.x >= xMin && vert.x <= xMax;
                if (!isOk)
                {
                    continue;
                }

                for (int z = 0; z < zCount; z++)
                {
                    var zMin = z * increment;
                    var zMax = (z + 1) * increment;
                    isOk = vert.z >= zMin && vert.z <= zMax;
                    if (!isOk)
                    {
                        continue;
                    }

                    for (int y = 0; y < yCount; y++)
                    {
                        var yMin = y * increment;
                        var yMax = (y + 1) * increment;
                        isOk = vert.y >= yMin && vert.y <= yMax;
                        if (!isOk)
                        {
                            continue;
                        }

                        var minVec = new Vector3(xMin, yMin, zMin);
                        var maxVec = new Vector3(xMax, yMax, zMax);
                        var tuple = new ValueTuple<Vector3, Vector3>(minVec, maxVec);
                        if (dict.ContainsKey(tuple))
                        {
                            dict[tuple].Add(vertMap);
                        }
                        else
                        {
                            dict.Add(tuple, new List<VertexMap>()
                            {
                                vertMap
                            });
                        }
                    }
                }
            }
        }

        return dict;
    }

    public struct VertexMap
    {
        public Vector3 Vertex;
        public int Index;

        public VertexMap(Vector3 vertex, int index)
        {
            Vertex = vertex;
            Index = index;
        }
    }
}  