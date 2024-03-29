using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G03 : G02
    {
        public G03(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }
        
        public override string Description => Locals.G03;

        public override async Task Execute(bool draw)
        {
            await base.Execute(true, draw);
        }

        public override void Execute2D()
        {
            base.Execute2D(true);
        }
    }
}