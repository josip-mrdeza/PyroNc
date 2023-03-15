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
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Simulation
{
    public static class ToolHelper
    {
        private static T Setter;
        public static ToolValues GetDefaultsOrCreate(this ToolBase toolBase)
        {
            return Globals.DefaultsManager.Values.Mutate(x =>
            {
                x.Storage = ValueStorage.CreateFromFile(toolBase);
                x.TokenSource = new CancellationTokenSource();
                return x;
            }) ?? new ToolValues(toolBase);
        }
        [Obsolete]
        public static async Task<ToolConfiguration> ChangeTool(this ToolBase toolBase, int index)
        {
            ThrowNoToolException(toolBase);
            Setter = new T(toolBase, new ArbitraryCommandParameters());
            Setter.Parameters.AddValue("value", index);
            await Setter.ExecuteFinal(true);

            return toolBase.ToolConfig;
        }

        public static void Pause(this MachineBase machine)
        {
            machine.StateControl.PauseControl();
        }

        public static void Resume(this MachineBase machine)
        {
            machine.StateControl.FreeControl();
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