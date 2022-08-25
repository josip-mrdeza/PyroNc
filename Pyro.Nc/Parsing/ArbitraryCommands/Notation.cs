using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Notation : BaseCommand
    {
        public Notation(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            
        }
        
        public long Number { get; set; }
        public override string Description { get => $"N{Number.ToString()}"; }
    }
}