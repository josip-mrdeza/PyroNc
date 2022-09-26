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

        public override async Task Execute(bool draw)
        {
            var flag0 = Parameters.Values.TryGetValue("S", out var ms);
            ms *= 1000f;
            if (!flag0)
            {
                Parameters.Values.TryGetValue("P", out ms);
            }

            var parameters = Parameters as MCommandParameters;
            for (int i = 0; i < ms; i++)
            {
                await Task.Yield();
                if (parameters!.Token.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(1);
                Tool.Values.IsAllowed = false;
            }

            Tool.Values.IsAllowed = true;
        }
    }
}