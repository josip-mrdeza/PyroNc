using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Pyro.Nc;
using Pyro.Nc.Parsing;

namespace PyroTests
{
    [MemoryDiagnoser]
    public class LinqTests
    {
        public string RandomCommand;
        public List<string[]> Commands;
        public LinqTests()
        {
            RandomCommand = Randomizer.GenerateRandomCommand();
            Commands = Randomizer.GenerateRandomCommands(3).Select(x => x.Split(' ')).ToList();
        }
        [Benchmark]
        public void FindVariables()
        {
            RandomCommand.FindVariables();
        }
        [Benchmark]
        public void CollectCommands()
        {
            Commands.CollectCommands();
        }
    }
}