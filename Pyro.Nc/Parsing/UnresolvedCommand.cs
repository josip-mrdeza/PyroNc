using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing
{
    public class UnresolvedCommand : BaseCommand
    {
        public UnresolvedCommand(ITool tool, ICommandParameters parameters, bool throwOnNull = false, Group family = Group.None) : base(tool, parameters, throwOnNull, family)
        {
            
        }
    }
}