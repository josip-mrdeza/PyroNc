using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G332 : G331    
    {
        public G332(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        
        public override string Description => Locals.G331;
    }
}