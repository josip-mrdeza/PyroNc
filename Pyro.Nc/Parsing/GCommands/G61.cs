using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G61 : G00
    {
        public G61(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description { get => Locals.G61; } 

        public override async Task Execute(bool draw)
        {
            Tool.ExactStopCheck = true;
        }
    }
}