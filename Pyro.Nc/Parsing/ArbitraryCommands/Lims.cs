using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Lims : BaseCommand
    {
        public Lims(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
            
        }
        public override string Description => Locals.Lims;

        public override Task Execute(bool draw)
        {
            var toolValues = Tool.Values;
            toolValues.SpindleSpeed.UpperLimit = Parameters.GetValue("value");
            Tool.Self.maxAngularVelocity = toolValues.SpindleSpeed.UpperLimit;

            return Task.CompletedTask;
        }
    }
}