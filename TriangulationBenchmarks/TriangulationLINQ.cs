using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using Pyro.IO;

namespace TriangulationBenchmarks
{
    [MemoryDiagnoser]
    public class TriangulationLINQ
    {
        public string Start = "The balls have been busted !";
        public char separator = ' ';
        [Benchmark]
        public void Contains()
        {
            _ = Start.Contains("Busted !");
        }

        [Benchmark]
        public void Contains_Custom()
        {
            _ = Start.ContainsFast("Busted !");
        }
    }
}