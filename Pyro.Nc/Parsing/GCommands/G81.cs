using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing.GCommands.Exceptions;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G81 : G01
    {
        public G81(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            await Tool.Traverse(ResolvePosition(), LineTranslationSmoothness.Rough, draw);
        }
    }     
}