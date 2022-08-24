using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G91 : BaseCommand
    {
        public G91(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description { get => Locals.G91; }

        public override Task Execute(bool draw)
        {
            Tool.IsIncremental = true;
            
            return Task.CompletedTask;
        }
    }
}