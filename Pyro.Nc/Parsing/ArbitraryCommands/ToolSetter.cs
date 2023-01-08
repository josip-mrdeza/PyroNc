using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class ToolSetter : BaseCommand
    {
        public ToolSetter(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            
        }

        public override string Description
        {
            get
            {
                try
                {
                    var value = (int) Parameters.GetValue("value");
                    var tool = Globals.ToolManager.Tools.FirstOrDefault(t => t.Index == value);
                    return $"{tool.Name} (R{tool.Radius.ToString(CultureInfo.InvariantCulture)})";
                }
                catch
                {
                    return "No such tool exists.";
                }
            }
        }
        public override async Task Execute(bool draw)
        {
            if (Globals.ToolManager is null)
            {
                throw new NullReferenceException("ToolSetter::Execute: Global parameter 'ToolManager' is null!");
            }
            var value = (int) Parameters.GetValue("value");
            var tool = Globals.ToolManager.Tools.FirstOrDefault(t => t.Index == value);
            if (tool == null)
            {
                throw new ToolMissingException(value);
            }

            Tool.ToolConfig = tool;
            Tool.Temp.transform.localPosition = new Vector3(0, Tool.ToolConfig.VerticalMargin);
            await Tool.EventSystem.FireAsync("ToolChange");
        }
    }
}