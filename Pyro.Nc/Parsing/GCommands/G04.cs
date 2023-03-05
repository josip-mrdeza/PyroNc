using System;
using System.Threading.Tasks;
using Pyro.IO;
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
            var flag0 = Parameters.Values.TryGetValue("S", out var ms);
            ms *= 1000f;
            if (!flag0)
            {
                Parameters.Values.TryGetValue("P", out ms);
            }

            Machine.StateControl.PauseControl();
            var timeSpan = TimeSpan.FromMilliseconds(ms);
            await Task.Delay(timeSpan);
            Machine.StateControl.FreeControl();
        }
    }
}