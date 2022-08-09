using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Trans : ABaseCommand
    {
        public Trans(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
        }
    }
}