using System;

namespace Pyro.IO.PyroSc.KeywordExceptions
{
    public class InvalidImportArgument : Exception
    {
        public InvalidImportArgument(string argument, Exception inner = null) : base("Import failed with the following argument: " + argument, inner)
        {
        }
    }
}