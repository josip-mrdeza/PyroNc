using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
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
        public override Task Execute(bool draw)
        {
            var value = (int) Parameters.GetValue("value");
            if (!Globals.ToolManager.Tools.Exists(t => t.Index == value))
            {
                throw new ToolMissingException(value);
            }

            Tool.ToolConfig = Globals.ToolManager.Tools.First(t => t.Index == value);
            return Task.CompletedTask;
        }
    }
}