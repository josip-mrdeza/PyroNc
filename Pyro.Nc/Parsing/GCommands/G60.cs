using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G60 : G00
    {
        public G60(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description { get => Locals.G60; } 

        public override async Task Execute(bool draw)
        {
            MemorySlots.Add(Tool.Position);
        }
    }
}