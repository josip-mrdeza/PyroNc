using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G81 : G01
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="parameters"></param>
        public G81(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.G81;

        public override async Task Execute(bool draw)
        {
            if(!float.IsNaN(Parameters.GetValue("X")) || !float.IsNaN(Parameters.GetValue("Y")))
            {
                throw new DrillParameterMismatchException(this);
            }
            await Tool.Traverse(ResolvePosition(), LineTranslationSmoothness.Rough, draw);
        }
    }     
}