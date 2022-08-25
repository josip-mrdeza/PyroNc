using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G70 : BaseCommand
    {
        public G70(ITool tool, ICommandParameters parameters, bool throwOnNull = false) : base(tool, parameters, throwOnNull)
        {
        }

        public override string Description => Locals.G70;

        public override Task Execute(bool draw)
        {
            Tool.IsImperial = true;
            return Task.CompletedTask;
        }
    }
}