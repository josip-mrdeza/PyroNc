using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G90 : BaseCommand
    {
        public G90(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G90;

        public override Task Execute(bool draw)
        {
            Machine.SimControl.Movement = MovementType.Absolute;
            
            return Task.CompletedTask;
        }
    }
}