using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Simulation
{
    public static class ToolHelper
    {
        private static ToolSetter Setter;
        public static ToolValues GetDefaultsOrCreate(this ITool tool)
        {
            return Globals.DefaultsManager.Values.Mutate(x =>
            {
                x.Storage = ValueStorage.CreateFromFile(tool);
                x.TokenSource = new CancellationTokenSource();
                return x;
            }) ?? new ToolValues(tool);
        }

        public static async Task<ToolConfiguration> ChangeTool(this ITool tool, int index)
        {
            if (tool is null)
            {
                throw new NullReferenceException("ChangeTool: Parameter 'tool' is null!");
            }
            Setter ??= new ToolSetter(tool, new ArbitraryCommandParameters());
            Setter.Parameters.AddValue("value", index);
            await Setter.ExecuteFinal(true);

            return tool.ToolConfig;
        }

        public static async Task Pause(this ITool tool)
        {
            if (tool is null)
            {
                throw new NullReferenceException("Pause: Parameter 'tool' is null!");
            }
            
            tool.Values.IsPaused = true;    
            await tool.EventSystem.FireAsync("ProgramPause");
        }

        public static Task Resume(this ITool tool)
        {
            if (tool is null)
            {
                throw new NullReferenceException("Pause: Parameter 'tool' is null!");
            }

            tool.Values.IsPaused = false;

            return Task.CompletedTask;
        }
    }
}