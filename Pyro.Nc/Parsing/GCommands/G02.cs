using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Exceptions;
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
            var arc = GetArc3D(reverse);
            await Tool.Traverse(arc, true);
            //Expire();
        }
        public override void Execute2D()
        {
            Execute2D(false);
        }

        protected void Execute2D(bool reverse)
        {
            Tool.TraverseFinal2D(GetArc3D(reverse).Points.ToArray());
        }
        private Arc3D GetArc3D(bool reverse)
        {
            //G2 I5 J5 X10 Y10
            // I = X AXIS CENTER POINT OFFSET
            // J = Y AXIS CENTER POINT OFFSET
            // X = ENDPOINT X AXIS
            // Y = ENDPOINT Y AXIS

            var parameters = (Parameters as GCommandParameters);
            var pos = Tool.Position;
            var trans = Tool.Values.TransPosition;

            //var transPosition = pos + trans;

            Vector3 endPoint;
            if (float.IsNaN(parameters.X) && float.IsNaN(parameters.Z))
            {
                endPoint = pos;
            }
            else
            {
                endPoint = new Vector3((parameters.X + trans.x).FixNan(pos.x), pos.y, (parameters.Z + trans.z).FixNan(pos.z));
            }

            if (float.IsNaN(parameters.I) && float.IsNaN(parameters.J))
            {
                throw NotifyException.Create<ErrorInEndPointOfCircleException>(
                    this, $"Cannot find arc center point from given values:" +
                    $"{string.Join("", parameters.Values.Where(x => !float.IsNaN(x.Value)).Select(y => $"{y.Key}{y.Value}").ToArray())}" +
                    $"\nTry defining the center point of arc with 'I' and 'J'.");
            }

            var centerPoint = pos + new Vector3(parameters.I.FixNan(), 0, parameters.J.FixNan());
            var radius = Vector3.Distance(pos, centerPoint);
            var arc = new Arc3D(radius, centerPoint, pos, endPoint, reverse);

            return arc;
        }
    }
}
