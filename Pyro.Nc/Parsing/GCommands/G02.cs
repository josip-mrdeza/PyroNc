using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G02 : G01
    {
        public G02(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override bool IsArc => true;

        public override string Description => Locals.G02;

        public override async Task Execute(bool draw)
        {
            await Execute(false, draw);
        }
        //TODO this only defines the beginning of the circle, not it's center as i thought before.This needs fixing...
        protected async Task Execute(bool reverse, bool draw)
        {
            var parameters = (Parameters as GCommandParameters);
            var pos = Tool.Position;
            var trans = Tool.Values.TransPosition;
            var altPos = new Vector3(pos.x + (float.IsNaN(parameters.I) ? 0 : parameters.I), 
                                     pos.y, 
                                     pos.z + (float.IsNaN(parameters.J) ? 0 : parameters.J)) + trans;
            var diff = Vector3.Distance(pos, altPos);
            var endPos = new Vector3(float.IsNaN(parameters.X) ? pos.x : parameters.X, 
                                      pos.y, 
                                      float.IsNaN(parameters.Z) ? pos.z : parameters.Z) + trans;
            var arc = new Circle3D(diff, pos.ToVector3D(), endPos.ToVector3D());
            await Tool.Traverse(arc, reverse, draw);
            //Expire();
        }
    }
}
