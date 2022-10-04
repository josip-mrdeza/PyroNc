using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G54 : BaseCommand
    {
        public G54(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            
        }
    }
}