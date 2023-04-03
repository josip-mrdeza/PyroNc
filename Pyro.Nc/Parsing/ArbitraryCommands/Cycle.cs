using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Cycle : BaseCommand
    {
        public Cycle(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public BaseCommand CurrentContext { get; private set; }
        
        public void SetOp(BaseCommand command)
        {
            CurrentContext = command;
        }
    }
}