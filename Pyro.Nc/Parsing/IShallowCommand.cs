using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing
{
    public interface IShallowCommand
    {
        public ITool Tool { get; set; }
        public string Description { get; }
    }
}