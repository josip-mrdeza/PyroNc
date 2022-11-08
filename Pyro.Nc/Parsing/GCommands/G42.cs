using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G42 : BaseCommand
    {
        public G42(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        
        public override Task Execute(bool draw)
        {
            return Task.CompletedTask;
        }
    }
}