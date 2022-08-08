using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M06 : M00
    {
        public M06(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            //TODO Change tool mesh and radius
        }
    }
}