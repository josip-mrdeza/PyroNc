using System;
using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M03 : BaseCommand
    {
        public M03(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M03;
        public bool IsNegative { get; protected set; }
        public override Task Execute(bool draw)
        {
            if (!Tool.IsPresent())
            {
                throw NotifyException.Create<ToolNotDefinedException>(this);
            }
            Tool.Self.angularVelocity = new Vector3(0, 1, 0) * ((IsNegative ? -1 : 1) * Tool.Values.SpindleSpeed * 0.10472f);
            return Task.CompletedTask;
        }
    }
}