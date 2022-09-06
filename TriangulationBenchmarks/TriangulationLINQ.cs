using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace TriangulationBenchmarks
{
    [MemoryDiagnoser]
    public class TriangulationLINQ
    {
        public static Vector3[] Vertices = new Vector3[21000];
        public static int[] Triangles = new int[30000];
        public static int[] Indices = new int[]
        
        {
            1,
            3,
            4,
            8,
            2,
            9,
            13,
            12,
            11,
            10
        };
        [Benchmark]
        public void TriangulateLINQ()
        {
            List<Vector3> verts = Vertices.ToList();
            foreach (var index in Indices)
            {
                verts.RemoveAt(index);
            }

            List<int> trigs = new List<int>(Triangles.Length);
            var ogTrigs = Triangles;
            for (int i = 0; i < ogTrigs.Length; i++)
            {
                var trigV0 = ogTrigs[i];
                var trigV1 = ogTrigs[i + 1];
                var trigV2 = ogTrigs[i + 2];

                if (Indices.All(c => trigV0 != c && trigV1 != c && trigV2 != c))
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }
            }

            var sorted = Indices.OrderBy(x => x).ToArray();

            var min = sorted.Min();
            var trigsArr1 = trigs.Select(t => t > min ? t - sorted.Length : t).ToArray();

            var vertices = verts.ToArray();
            var trigsArr = trigsArr1.Concat(trigsArr1.ToArray().Reverse()).ToArray();
        }
        
        [Benchmark]
        public void TriangulateNonLINQ()
        {
            List<Vector3> verts = Vertices.ToList();
            foreach (var index in Indices)
            {
                verts.RemoveAt(index);
            }

            List<int> trigs = new List<int>(Triangles.Length);
            var ogTrigs = Triangles;
            for (int i = 0; i < ogTrigs.Length; i++)
            {
                var trigV0 = ogTrigs[i];
                var trigV1 = ogTrigs[i + 1];
                var trigV2 = ogTrigs[i + 2];

                bool all = true;
                foreach (var c in Indices)
                {
                    if (trigV0 == c || trigV1 == c || trigV2 == c)
                    {
                        all = false;

                        break;
                    }
                }

                if (all)
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }
            }

            var sorted = Indices.OrderBy(x => x).ToArray();

            var min = sorted.Min();
            var trigsArr1 = trigs.Select(t => t > min ? t - sorted.Length : t).ToArray();

            var vertices = verts.ToArray();
            var trigsArr = trigsArr1.Concat(trigsArr1.ToArray().Reverse()).ToArray();
        }
    }
}