using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M03 : BaseCommand
    {
        public M03(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M03;
        public bool IsNegative { get; set; }
        public override async Task Execute(bool draw)
        {
            Tool.Self.angularVelocity = new Vector3(0, 1, 0) * ((IsNegative ? -1 : 1) * Tool.Values.SpindleSpeed);
        }
    }
}