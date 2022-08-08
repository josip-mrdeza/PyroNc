using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G02 : G01
    {
        public G02(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override bool IsArc { get; }

        public override string Description
        {
            get => Locals.G02;
        }
        
        public override async Task Execute(bool draw)
        {
            await Execute(false, draw);
        }

        protected async Task Execute(bool reverse, bool draw)
        {
            var parameters = (Parameters as GCommandParameters);
            var pos = Tool.Position;
            var altPos = new Vector3(pos.x + parameters.I, pos.y, pos.z + parameters.J);
            var diff = Space3D.Distance(pos.ToVector3D(), altPos.ToVector3D());
            await Tool.Traverse(altPos, diff, reverse, draw);
            Expire();
        }
        
        public override ICommand Copy()
        {
            return this.MemberwiseClone() as G02;
        }
    }
}