using System;
using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Notation : Comment
    {
        public Notation(ToolBase toolBase, ArbitraryCommandParameters parameters) : base(toolBase, parameters)
        {
            
        }
        
        public long Number { get; set; }
        public override string Description => $"N{Number.ToString()}";
    }
}