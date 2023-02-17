using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G05 : BaseCommand
    {
        public G05(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G05;
    }
}