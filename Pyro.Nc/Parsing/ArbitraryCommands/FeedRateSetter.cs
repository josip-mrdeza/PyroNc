using System;
using System.Threading.Tasks;
using Pyro.Math;
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
            Tool.Values.FeedRate.Set(Parameters.GetValue("value"));
            Tool.Values.FastMoveTick = TimeSpan.FromMilliseconds((Parameters.GetValue("value").Pow(-1).Squared()));

            return Task.CompletedTask;
        }
    }
}