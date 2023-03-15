using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G00 : BaseCommand
    {
        public G00(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters, false, Group.GCommand)
        {
        }

        public override bool IsModal => true;
        /// <inheritdoc />
        public override string Description => Locals.G00;
        /// <inheritdoc />
        public override async Task Execute(bool draw)
        {
            await ToolBase.Traverse(ResolvePosition(), Parameters.LineSmoothness, draw);
        }

        public override void Execute2D()
        {
            ToolBase.Traverse2D(ResolvePosition(), Parameters.LineSmoothness);
        }

        /// <summary>
        /// Resolves the position in which ever mode is set at the time. (Incremental / Absolute [Default])
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LinearInterpolationParameterMismatchException">This exception is thrown when the <see cref="ICommandParameters"/>
        /// passed to this <see cref="ICommand"/> are zeroed out.</exception>
        public virtual Vector3 ResolvePosition()
        {
            var parameters = (Parameters as GCommandParameters);
            Vector3 point;
            var trans = Machine.SimControl.WorkOffset + Machine.SimControl.Trans;
            var x = parameters.GetValue("X");
            var y = parameters.GetValue("Y");
            var z = parameters.GetValue("Z");
            if (Machine.SimControl.Movement == MovementType.Incremental)
            {
                if ((ResolveNan(x, 0) == 0 && ResolveNan(y, 0) == 0 && ResolveNan(z, 0) == 0))
                {
                    throw new LinearInterpolationParameterMismatchException(this);
                }

                var pos = ToolBase.Position;
                point = new Vector3(pos.x + ResolveNan(x, 0),
                                    pos.y + ResolveNan(y, 0), 
                                    pos.z + ResolveNan(z, 0));
            }
            else
            {
                var pos = ToolBase.Position;
                point = new Vector3((x + trans.x).FixNan(pos.x),
                                    (y + trans.y).FixNan(pos.y),
                                    (z + trans.z).FixNan(pos.z));

            }

            return point;
        }
    }
}
