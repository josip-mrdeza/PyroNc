using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G64 : G00
    {
        public G64(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            await Tool.InvokeOnConsumeStopCheck();
            Tool.ExactStopCheck = false;
        }
    }
}