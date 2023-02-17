using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G81 : G01
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolBase"></param>
        /// <param name="parameters"></param>
        public G81(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G81;

        public override async Task Execute(bool draw)
        {
            if(!float.IsNaN(Parameters.GetValue("X")) || !float.IsNaN(Parameters.GetValue("Z")))
            {
                throw new DrillParameterMismatchException(this);
            }

            var pos = ToolBase.Position;
            pos.y = ((GCommandParameters)Parameters).Y;
            await ToolBase.Traverse(pos, LineTranslationSmoothness.Rough, draw);
        }
    }     
}