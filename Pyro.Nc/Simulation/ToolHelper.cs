using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Simulation
{
    public static class ToolHelper
    {
        private static ToolSetter Setter;
        public static ToolValues GetDefaultsOrCreate(this ToolBase toolBase)
        {
            return Globals.DefaultsManager.Values.Mutate(x =>
            {
                x.Storage = ValueStorage.CreateFromFile(toolBase);
                x.TokenSource = new CancellationTokenSource();
                return x;
            }) ?? new ToolValues(toolBase);
        }

        public static async Task<ToolConfiguration> ChangeTool(this ToolBase toolBase, int index)
        {
            ThrowNoToolException(toolBase);
            Setter = new ToolSetter(toolBase, new ArbitraryCommandParameters());
            Setter.Parameters.AddValue("value", index);
            await Setter.ExecuteFinal(true);

            return toolBase.ToolConfig;
        }

        public static bool IsPresent(this ToolBase toolBase)
        {
            ThrowNoToolException(toolBase);

            return toolBase.ToolConfig.Index != 0;
        }

        public static async Task Pause(this ToolBase toolBase)
        {
            ThrowNoToolException(toolBase);

            //toolBase.Values.IsPaused = true;    
            //await toolBase.EventSystem.FireAsync("ProgramPause");
        }

        public static Task Resume(this ToolBase toolBase)
        {
            ThrowNoToolException(toolBase);

            //toolBase.Values.IsPaused = false;

            return Task.CompletedTask;
        }
        
        public static void ThrowNoToolException(this ToolBase toolBase)
        {
            if (toolBase is null)
            {
                throw new NotifyException("ChangeTool: Parameter 'tool' is null!");
            }
        }
    }
}