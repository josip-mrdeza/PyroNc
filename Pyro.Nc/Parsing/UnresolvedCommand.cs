using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using ToolBase = Pyro.Nc.Simulation.Tools.ToolBase;

namespace Pyro.Nc.Parsing
{
    public class UnresolvedCommand : BaseCommand
    {
        public UnresolvedCommand(ToolBase tool, ICommandParameters parameters) : base(tool, parameters, false, Group.None)
        {
            
        }
    }
}