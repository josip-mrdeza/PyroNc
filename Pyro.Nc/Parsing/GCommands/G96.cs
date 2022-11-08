using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G96 : BaseCommand
    {
        public G96(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        
        public override Task Execute(bool draw)
        {
            return Task.CompletedTask;
        }
    }
}