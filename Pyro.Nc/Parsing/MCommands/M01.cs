using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M01 : M00
    {
        public M01(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public string Description { get => Locals.M01; }

        public override async Task Execute(bool draw)
        {
            //if (optionalButtonPressed) //TODO This requies Pyro.Nc to interop into the machine, which I do not know how to do from .NET yet.
            //{
                await base.Execute(draw);
            //}
        }
    }
}