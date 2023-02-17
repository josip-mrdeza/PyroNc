using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G61 : BaseCommand
    {
        public G61(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }
        public override string Description => Locals.G61;

        public override Task Execute(bool draw)
        {
            ToolBase.Values.ExactStopCheck = true;

            return Task.CompletedTask;
        }
    }
}