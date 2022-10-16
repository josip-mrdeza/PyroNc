using System.Collections;
using System.Collections.Generic;

namespace Pyro.Nc.Parsing
{
    public static class Parser
    {
        private enum BlockType
        {
            Unknown,
            GCommand,
            MCommand,
            ACommand,
            Parameter
        }
        private class Block
        {
            public Block(string text, BlockType @group)
            {
                Text = text;
                Group = @group;
            }
            
            public string Text;
            public BlockType Group;
        }
        /// <summary>
        /// Find all commands in a line, return blocks of commands
        /// </summary>
        public static IEnumerable<string> FindAll(string s)
        {
            List<Block> blocks = new List<Block>();
            string[] splitStrings = s.Split(' ');

            for (int i = 0; i < splitStrings.Length; i++)
            {
                var value = splitStrings[i];
                blocks.Add(value[0] switch
                {
                    'G' => new Block(value, BlockType.GCommand),
                    'M' => new Block(value, BlockType.MCommand),
                      _ => new Block(value, BlockType.Unknown)
                });
            }

            return null;
        }
    }
}