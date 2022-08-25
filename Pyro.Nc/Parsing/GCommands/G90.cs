using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G90 : BaseCommand
    {
        public G90(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.G90;

        public override Task Execute(bool draw)
        {
            Tool.IsIncremental = false;
            
            return Task.CompletedTask;
        }
    }
}