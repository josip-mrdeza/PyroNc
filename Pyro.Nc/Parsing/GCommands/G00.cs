using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing.GCommands.Exceptions;
using Pyro.Nc.Pathing;
using TrCore;
using TrCore.Logging;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G00 : BaseCommand
    {
        public G00(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }       
        
        public override string Description => Locals.G00;

        public override async Task Execute(bool draw)
        {
            await Tool.Traverse(ResolvePosition(), LineTranslationSmoothness.Rough, draw);
            Expire();
        }

        public override void Expire()
        {
            Tool.CurrentPath.Expired = true;
        }
        
        public virtual Vector3 ResolvePosition()
        {
            var parameters = (Parameters as GCommandParameters);
            Vector3 point;
            if (Tool.IsIncremental)
            {
                if (parameters.X == 0 && parameters.Y == 0 && parameters.Z == 0)
                {
                    throw new LinearInterpolationParameterMismatchException(this);
                }

                point = Tool.Position + new Vector3(parameters.X, parameters.Y, parameters.Z);
            }
            else
            {
                point = new Vector3(parameters.X, parameters.Y, parameters.Z);
            }

            return point;
        }
    }
}