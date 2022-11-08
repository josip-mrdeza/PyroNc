using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M05 : BaseCommand
    {
        public M05(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M05;

        public override Task Execute(bool draw)
        {
            Tool.Self.angularVelocity = Vector3.zero;

            return Task.CompletedTask;
        }
    }
}