using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G64 : BaseCommand
    {
        public G64(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.G64;

        public override async Task Execute(bool draw)
        {
            await Tool.InvokeOnConsumeStopCheck();
            Tool.ExactStopCheck = false;
        }
    }
}