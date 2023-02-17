using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing
{
    public interface IShallowCommand
    {
        public ToolBase ToolBase { get; set; }
        public Group Family { get; set; }
        public string Description { get; }
    }
}