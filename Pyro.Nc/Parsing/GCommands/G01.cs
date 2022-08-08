using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G01 : G00
    {
        public G01(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description
        {
            get => Locals.G01;
        }
    }
}