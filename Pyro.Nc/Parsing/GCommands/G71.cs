using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G71 : BaseCommand  
    {
        public G71(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G71;

        public override Task Execute(bool draw)
        {
            Machine.SimControl.Unit = UnitType.Metric;
            return Task.CompletedTask;
        }
    }
}