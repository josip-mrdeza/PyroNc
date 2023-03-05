using System;
using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M03 : BaseCommand
    {
        public M03(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }
        public override string Description => Locals.M03;
        public bool IsNegative { get; protected set; }
        public override Task Execute(bool draw)
        {
            ToolBase.ThrowNoToolException();
            Machine.SetSpindleSpeed((IsNegative ? -1 : 1) * Machine.SpindleControl.SpindleSpeed);
            return Task.CompletedTask;
        }
    }
}