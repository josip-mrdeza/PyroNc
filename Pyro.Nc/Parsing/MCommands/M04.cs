using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M04 : M03
    {
        public M04(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public string Description { get => Locals.M04; }

        public override async Task Execute(bool draw)
        { 
            //TODO make it change the spindle counter clockwise rotation speed...
            throw new NotImplementedException();
        }
    }
}