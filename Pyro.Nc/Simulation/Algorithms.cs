using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
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

    public static VertexHashMapGenerationTestOutput TestVertexHashMapGeneration(int iterations, float step)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int boxes = 0;
        int ranges = 0;
        double ms = 0;
        for (int i = 0; i < iterations; i++)
        {
            MachineBase.CurrentMachine.Workpiece.GenerateVertexBoxHashes(step, HashmapGenerationReason.Test);
            stopwatch.Stop();
            ms = (ms + stopwatch.Elapsed.TotalMilliseconds) / 2;
            ranges = (ranges + MachineBase.CurrentMachine.Workpiece.VertexBoxHash.Count) / 2;
            boxes = (boxes + MachineBase.CurrentMachine.Workpiece.VertexBoxHash.Sum(x => x.Value.Count)) / 2;
            stopwatch.Restart();
        }
        stopwatch.Stop();

        return new VertexHashMapGenerationTestOutput(iterations, ranges, boxes, step, (float)ms);
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
    
    public class VertexHashMapGenerationTestOutput
    {
        public int Iterations { get; set; }
        public int BoxesCreated { get; set; }
        public int RangesCreated { get; set; }
        public float Step { get; set; }
        public float AverageMsPerOperation { get; set; }

        public VertexHashMapGenerationTestOutput(int iterations, int rangesCreated, int boxesCreated, float step, float averageMsPerOperation)
        {
            Iterations = iterations;
            RangesCreated = rangesCreated;
            BoxesCreated = boxesCreated;
            Step = step;
            AverageMsPerOperation = averageMsPerOperation;
        }
    }
}  