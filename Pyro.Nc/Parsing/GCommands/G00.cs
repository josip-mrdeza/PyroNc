using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using TrCore;
using TrCore.Logging;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G00 : BaseCommand
    {
        public G00(ITool tool, GCommandParameters parameters) : base(tool, parameters, false, Group.GCommand)
        {
        }

        public override bool IsModal => true;
        /// <inheritdoc />
        public override string Description => Locals.G00;
        /// <inheritdoc />
        public override async Task Execute(bool draw)
        {
            if (Tool.ToolConfig.Index == -1)
            {
                throw new ToolNotDefinedException();
            }
            if (Tool.Values.FeedRate == 0)
            {
                throw new FeedRateNotDefinedException();
            }
            await Tool.Traverse(ResolvePosition(), Parameters.LineSmoothness, draw);
        }
        /// <summary>
        /// Resolves the position in which ever mode is set at the time. (Incremental / Absolute [Default])
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LinearInterpolationParameterMismatchException">This exception is thrown when the <see cref="ICommandParameters"/>
        /// passed to this <see cref="ICommand"/> are zeroed out.</exception>
        public Vector3 ResolvePosition()
        {
            var parameters = (Parameters as GCommandParameters);
            Vector3 point;
            var trans = Tool.Values.TransPosition;
            if (Tool.Values.IsIncremental)
            {
                if ((ResolveNan(parameters.X, 0) == 0 && ResolveNan(parameters.Y, 0) == 0 && ResolveNan(parameters.Z, 0) == 0))
                {
                    throw new LinearInterpolationParameterMismatchException(this);
                }

                var pos = Tool.Position;
                point = new Vector3(pos.x + ResolveNan(parameters.X, 0),
                                    pos.y + ResolveNan(parameters.Y, 0), 
                                    pos.z + ResolveNan(parameters.Z, 0));
            }
            else
            {
                var pos = Tool.Position;
                point = new Vector3(ResolveNan(parameters.X, pos.x),
                                    ResolveNan(parameters.Y, pos.y),
                                    ResolveNan(parameters.Z, pos.z))  + trans;
            }

            return point;
        }
    }
}