using System;
using System.Linq;
using System.Reflection;
using Pyro.IO.PyroSc.KeywordExceptions;

namespace Pyro.IO.PyroSc.Keywords
{
    internal class Import : Keyword
    {
        public Import(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
        }

        public override object Run()
        {
            var elementAhead = NextAsString();
            try
            {
                Assembly.LoadFrom(elementAhead);
            }
            catch (Exception e)
            {
                throw new InvalidImportArgument(elementAhead, e);
            }

            return null;
        }
    }
}