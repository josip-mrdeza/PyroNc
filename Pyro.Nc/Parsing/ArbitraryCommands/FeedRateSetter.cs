using System;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class FeedRateSetter : BaseCommand
    {
        public FeedRateSetter(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.FeedRateSetter;

        public override Task Execute(bool draw)
        {
            var value = Parameters.GetValue("value");

            var feedRate = Tool.Values.FeedRate;
            if (feedRate.UpperLimit < value)
            {
                throw new FeedRateOverLimitException(this, value, feedRate.UpperLimit);
            }
            feedRate.Set(value);
            Tool.InvokeOnFeedRateChanged(value);
            return Task.CompletedTask;
        }
    }
}