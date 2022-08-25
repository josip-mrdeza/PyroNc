using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G331 : G81
    {
        public G331(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
    }
}