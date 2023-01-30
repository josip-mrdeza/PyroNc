using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing
{
    public interface IShallowCommand
    {
        public MillTool3D Tool { get; set; }
        public Group Family { get; set; }
        public string Description { get; }
    }
}