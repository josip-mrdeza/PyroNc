using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M05 : BaseCommand
    {
        public M05(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }
        public override string Description => Locals.M05;

        public override Task Execute(bool draw)
        {
            Machine.SetSpindleSpeed(0);
            return Task.CompletedTask;
        }
    }
}