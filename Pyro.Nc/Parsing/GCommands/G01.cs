using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands.Exceptions;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G01 : G00
    {
        public G01(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        
        public override string Description => Locals.G01;
    }
}