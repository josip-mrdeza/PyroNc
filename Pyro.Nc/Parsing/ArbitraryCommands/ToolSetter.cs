using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class ToolSetter : BaseCommand
    {
        public ToolSetter(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.ToolSetter;
        public override async Task Execute(bool draw)
        {
            //TODO Change the tool object & replace radius.
        }
    }
}