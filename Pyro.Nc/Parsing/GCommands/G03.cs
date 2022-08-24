using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G03 : G02
    {
        public G03(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        
        public override string Description
        {
            get => Locals.G03;
        }
        
        public override async Task Execute(bool draw)
        {
            await base.Execute(true, draw);
        }
    }
}