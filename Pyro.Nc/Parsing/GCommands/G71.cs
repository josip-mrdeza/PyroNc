using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G71 : BaseCommand  
    {
        public G71(ITool tool, ICommandParameters parameters, bool throwOnNull = false) : base(tool, parameters, throwOnNull)
        {
        }

        public override string Description => Locals.G71;

        public override Task Execute(bool draw)
        {
            Tool.IsImperial = false;
            return Task.CompletedTask;
        }
    }
}