using System;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M00 : BaseCommand
    {
        public M00(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
            Tool = tool;
            Parameters = parameters;
        }
        public override string Description => Locals.M00;

        public override Task Execute(bool draw)
        {
            var flag0 = Parameters.Values.TryGetValue("S", out var ms);
            ms *= 1000f;
            if (!flag0)
            {
                Parameters.Values.TryGetValue("P", out ms);
            }

            Tool.Values.IsAllowed = false;
            var timeSpan = TimeSpan.FromMilliseconds(ms);
            return Task.Delay(timeSpan).ContinueWith(x =>
            {
                Tool.Values.IsAllowed = true;
                return x;
            });
        }
    }
}