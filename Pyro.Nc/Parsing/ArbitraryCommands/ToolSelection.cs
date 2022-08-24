using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class ToolSelection : BaseCommand
    {
        public ToolSelection(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            //TODO Change the tool object & replace radius.
        }
    }
}