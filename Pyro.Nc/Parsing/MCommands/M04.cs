using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M04 : M03
    {
        public M04(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
            IsNegative = true;
        }
        public override string Description => Locals.M04;
    }
}