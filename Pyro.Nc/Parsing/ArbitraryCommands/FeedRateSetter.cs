using System;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class FeedRateSetter : BaseCommand
    {
        public FeedRateSetter(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
            Machine.SpindleControl.FeedRate.SetUpperValue(350);
        }

        public override string Description => Locals.FeedRateSetter;

        public override Task Execute(bool draw)
        {
            var value = Parameters.GetValue("value");

            var feedRate = Machine.SpindleControl.FeedRate;
            if (feedRate.UpperLimit < value)
            {
                throw new FeedRateOverLimitException(this, value, feedRate.UpperLimit);
            }
            Machine.SetFeedRate(value);
            return Task.CompletedTask;
        }
    }
}