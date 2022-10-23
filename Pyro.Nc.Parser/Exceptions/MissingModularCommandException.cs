using System;

namespace Pyro.Nc.Parser.Exceptions
{
    public class MissingModularCommandException : Exception
    {
        public MissingModularCommandException(Block block) : base($"In line {block.Line} -> block '{block.Text}'")
        {
            base.Data.Add("b", block);
        }
    }
}