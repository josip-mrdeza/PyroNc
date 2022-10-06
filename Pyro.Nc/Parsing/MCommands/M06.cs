using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M06 : M00
    {
        public M06(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M06;

        public override async Task Execute(bool draw)
        {
            //??
            //TODO Change tool mesh and radius
            var tools = Globals.ToolManager.Tools;
            var toolIndex = tools.FirstOrDefault(x => x.Radius == Tool.Values.Radius).Index;
            var toolSetter = new ToolSetter(Tool, new ArbitraryCommandParameters()
            {
                Values = new Dictionary<string, float>()
                {
                    {
                        "values", toolIndex + 1
                    }
                }
            });
            await toolSetter.Execute(draw);
        }
    }
}