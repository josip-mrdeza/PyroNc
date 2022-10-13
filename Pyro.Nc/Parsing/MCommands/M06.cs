using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
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
            ToolConfiguration first = null;
            var requiredRadius = Parameters.GetValue("T");
            foreach (var x in tools)
            {
                if (x.Radius == requiredRadius)
                {
                    first = x;

                    break;
                }
            }

            if (first == null)
            {
                //throw new ToolMissingException(index);
            }
            var toolIndex = first.Index;
            var toolSetter = new ToolSetter(Tool, new ArbitraryCommandParameters()
            {
                Values = new Dictionary<string, float>()
                {
                    {
                        "values", toolIndex
                    }
                }
            });
            await toolSetter.Execute(draw);
        }
    }
}