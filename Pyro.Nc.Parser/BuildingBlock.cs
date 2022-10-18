using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pyro.Nc.Parser
{
    public class BuildingBlock
    {
        internal BuildingBlock(Block self, IEnumerable<Block> parameters)
        {
            Self = self;
            Parameters = parameters;
        }
        public Block Self { get; }
        public IEnumerable<Block> Parameters { get; }
        public IEnumerable<Block> Full { get; internal set; }

        public override string ToString()
        {
            return Self.Text;
        }
    }
}