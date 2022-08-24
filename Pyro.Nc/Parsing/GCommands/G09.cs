using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G09 : BaseCommand
    {
        public G09(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
            Tool.OnConsumeStopCheck += Run;
        }
        
        public override async Task Execute(bool draw)
        {
            Tool.ExactStopCheck = false;
            Tool.OnConsumeStopCheck -= Run;
        }
        
        private async Task Run()
        {
            await Execute(false);
        }
    }
}