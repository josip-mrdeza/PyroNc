using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G91 : BaseCommand
    {
        public G91(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G91;

        public override Task Execute(bool draw)
        {
            Machine.SimControl.Movement = MovementType.Incremental;
            return Task.CompletedTask;
        }
    }
}