using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing
{
    public class UnresolvedCommand : BaseCommand
    {
        public UnresolvedCommand(ITool tool, ICommandParameters parameters) : base(tool, parameters, false, Group.None)
        {
            
        }
    }
}