using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G41 : BaseCommand
    {
        public G41(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override Task Execute(bool draw)
        {
            return Task.CompletedTask;
        }
    }
}