using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G05 : G04
    {
        public G05(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description { get => Locals.G05; }
    }
}