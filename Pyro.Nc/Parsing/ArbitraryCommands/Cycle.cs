using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Cycle : BaseCommand
    {
        public Cycle(ITool tool, ICommandParameters parameters, bool throwOnNull = false, Group family = Group.None) : base(tool, parameters, throwOnNull, family)
        {
            
        }
        
        public string CycleId { get; set; }
    }
}