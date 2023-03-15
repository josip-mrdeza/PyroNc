using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class LIMS : BaseCommand
    {
        public LIMS(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
            
        }
        public override string Description => Locals.Lims;

        public override Task Execute(bool draw)
        {
            var spindle = Machine.SpindleControl;
            spindle.SpindleSpeed.SetUpperValue(Parameters.GetValue("value"));
            //ToolBase.Self.maxAngularVelocity = toolValues.SpindleSpeed.UpperLimit;

            return Task.CompletedTask;
        }
    }
}