using System;

namespace Pyro.Nc.Parser.Exceptions
{
    public class MissingModularCommandException : Exception
    {
        public MissingModularCommandException(Block block) : base(block.Text + $" // " + block.AdditionalInfo)
        {
            
        }
    }
}