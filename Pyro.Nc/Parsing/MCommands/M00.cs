using System;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    /// <summary>
    /// This is called either through code or through ToolHelper.Pause()
    /// </summary>
    public class M00 : BaseCommand
    {
        public M00(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
            ToolBase = toolBase;
            Parameters = parameters;
        }
        public override string Description => Locals.M00;

        public override async Task Execute(bool draw)
        {
            await ToolBase.Pause();
        }
    }
}