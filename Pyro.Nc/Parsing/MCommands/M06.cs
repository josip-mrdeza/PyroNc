using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M06 : BaseCommand
    {
        public M06(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }
        public override string Description => Locals.M06;
        public override async Task Execute(bool draw)
        {
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
            
            var rpp = Globals.ReferencePointParser.ToolMountReferencePoint;
            // var g0ToToolSetPosition = ToolBase.Values.Storage.FetchGCommand("G00") as G00;
            // g0ToToolSetPosition.Parameters = new GCommandParameters(rpp.x, rpp.y, rpp.z);
            // Globals.Rules.Try(g0ToToolSetPosition);
            // Expire();
            // await g0ToToolSetPosition.ExecuteFinal(draw);
            ToolBase.Position = rpp;
            var toolSetter = new T(ToolBase, new ArbitraryCommandParameters()
            {
                Values = new Dictionary<string, float>()
                {
                    {
                        "value", requiredTool
                    }
                }
            });
            await toolSetter.ExecuteFinal(draw);
        }
    }
}