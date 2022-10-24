using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M01 : M00
    {
        public M01(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M01;

        public override async Task Execute(bool draw)
        {
            await Tool.EventSystem.FireAsync("ProgramPauseConditional");
        }
    }
}