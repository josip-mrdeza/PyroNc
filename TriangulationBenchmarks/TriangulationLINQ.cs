using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using Pyro.IO;
using Pyro.IO.Memory;

namespace TriangulationBenchmarks
{
    [MemoryDiagnoser]
    public class TriangulationLINQ
    {
        public SharedMemory Memory = new SharedMemory("shared", 1_000_000, 1_000_000);
        
        [Benchmark]
        public void ReadMemory()
        {
        }
        
        [Benchmark]
        public void WriteMemory()
        {
        }
    }
}