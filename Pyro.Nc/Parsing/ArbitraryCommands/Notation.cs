using System;
using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Notation : Comment
    {
        public Notation(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            
        }
        
        public long Number { get; set; }
        public override string Description => $"N{Number.ToString()}";
    }
}