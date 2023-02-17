using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G09 : BaseCommand
    {
        public G09(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
            if (toolBase is null)
            {
                return;
            }
            //ToolBase.OnConsumeStopCheck += Run;
        }

        public override string Description => Locals.G09;

        public override async Task Execute(bool draw)
        {
            ToolBase.Values.ExactStopCheck = false;
            //ToolBase.OnConsumeStopCheck -= Run;
        }
        
        private async Task Run()
        {
            await Execute(false);
        }
    }
}