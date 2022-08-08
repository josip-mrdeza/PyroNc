using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M05 : M00
    {
        public M05(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public string Description { get => Locals.M05; }

        public override async Task Execute(bool draw)
        {
            //TODO set spindle speed to 0
            throw new NotImplementedException();
        }
    }
}