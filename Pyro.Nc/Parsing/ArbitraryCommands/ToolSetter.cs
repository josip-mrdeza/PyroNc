using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

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
            var value = (int) Parameters.GetValue("value");
            Tool.ToolConfig = Globals.ToolManager.Tools[value];
        }
    }
}