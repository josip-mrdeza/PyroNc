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
            await Tool.EventSystem.FireAsync("ProgramPause");
        }
    }
}