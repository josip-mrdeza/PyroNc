using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G70 : BaseCommand
    {
        public G70(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G70;

        public override Task Execute(bool draw)
        {
            Machine.SimControl.Unit = UnitType.Imperial;
            return Task.CompletedTask;
        }
    }
}