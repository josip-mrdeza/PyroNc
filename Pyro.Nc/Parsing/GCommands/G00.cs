using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
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
        
        public override string Description
        {
            get => Locals.G00;
        }

        public override async Task Execute(bool draw)
        {
            await Tool.Traverse(new Vector3D(Parameters.GetValue("X"), Parameters.GetValue("Y"), Parameters.GetValue("Z")).ToVector3(),
                                LineTranslationSmoothness.Rough, draw);
            Expire();
        }

        public override void Expire()
        {
            Tool.CurrentPath.Expired = true;
        }
    }
}