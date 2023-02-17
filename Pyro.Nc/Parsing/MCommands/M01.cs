using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M01 : M00
    {
        public M01(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }
        public override string Description => Locals.M01;

        public override async Task Execute(bool draw)
        {
            await Machine.EventSystem.PEvents.FireAsync("ProgramPauseConditional");
        }
    }
}