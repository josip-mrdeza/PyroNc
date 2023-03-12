using System;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G04 : BaseCommand
    {
        public G04(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G04;
        public override bool IsModal => true;

        public override async Task Execute(bool draw)
        {
            string parameter = "P";
            var hasParameter = Parameters.Values.TryGetValue(parameter, out var ms);
            if (!hasParameter)
            {
                throw new ParameterMissingException(parameter);
            }

            Machine.StateControl.PauseControl();
            var timeSpan = TimeSpan.FromMilliseconds(ms);
            await Task.Delay(timeSpan);
            Machine.StateControl.FreeControl();
        }
    }
}