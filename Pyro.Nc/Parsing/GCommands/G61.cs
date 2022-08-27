using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G61 : BaseCommand
    {
        public G61(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.G61;

        public override Task Execute(bool draw)
        {
            Tool.Values.ExactStopCheck = true;

            return Task.CompletedTask;
        }
    }
}