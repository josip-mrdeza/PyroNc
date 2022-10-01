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
            await Tool.Traverse(ResolvePosition(), Parameters.LineSmoothness, draw);
            Expire();
        }

        public override void Expire()
        {
            Tool.Values.CurrentPath.Expired = true;
        }
        
        public virtual Vector3 ResolvePosition()
        {
            var parameters = (Parameters as GCommandParameters);
            Vector3 point;
            if (Tool.Values.IsIncremental)
            {
                if (parameters.X == 0 && parameters.Y == 0 && parameters.Z == 0)
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
                                    ResolveNan(parameters.Z, pos.z));
            }

            return point;
        }

        public float ResolveNan(float val, float defaultVal)
        {
            if (float.IsNaN(val))
            {
                return defaultVal;
            }

            return val;
        }
    }
}