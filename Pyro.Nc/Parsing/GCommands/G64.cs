using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G64 : BaseCommand
    {
        public G64(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G64;

        public override async Task Execute(bool draw)
        {
            //await ToolBase.InvokeOnConsumeStopCheck();
            ToolBase.Values.ExactStopCheck = false;
        }
    }
}