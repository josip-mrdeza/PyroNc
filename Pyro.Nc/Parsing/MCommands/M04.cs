using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M04 : M03
    {
        public M04(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
            IsNegative = true;
        }
        public override string Description => Locals.M04;
    }
}