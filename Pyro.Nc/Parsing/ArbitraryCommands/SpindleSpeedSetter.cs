using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class SpindleSpeedSetter : BaseCommand
    {
        public SpindleSpeedSetter(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.SpindleSpeedSetter;

        public override Task Execute(bool draw)
        {
            Tool.Values.SpindleSpeed.Set(Parameters.GetValue("value"));

            return Task.CompletedTask;
        }
    }
}