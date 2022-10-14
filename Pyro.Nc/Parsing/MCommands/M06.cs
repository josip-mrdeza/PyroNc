using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

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
            var requiredTool = (int) Parameters.GetValue("value");
            foreach (var x in tools)
            {
                if (x.Index == requiredTool)
                {
                    first = x;

                    break;
                }
            }

            if (first == null)
            {
                throw new ToolMissingException(requiredTool);
            }
            
            var rpp = ReferencePointParser.ToolMountReferencePoint;
            var g0ToToolSetPosition = Tool.Values.Storage.FetchGCommand("G00") as G00;
            g0ToToolSetPosition.Parameters = new GCommandParameters(rpp.x, rpp.y, rpp.z);
            Globals.Rules.Try(g0ToToolSetPosition);
            await g0ToToolSetPosition.Execute(draw);
            var toolSetter = new ToolSetter(Tool, new ArbitraryCommandParameters()
            {
                Values = new Dictionary<string, float>()
                {
                    {
                        "value", requiredTool
                    }
                }
            });
            await toolSetter.Execute(draw);
        }
    }
}