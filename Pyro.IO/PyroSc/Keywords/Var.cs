using System;
using System.Linq;

namespace Pyro.IO.PyroSc.Keywords
{
    public class Var : ReferenceKeyword
    {
        public Var(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
        }                 

        private const string ExpectedAssignmentOperator = "=";
        public override object Run()
        {
            if (!HasAssignmentOperator())
            {
                throw new NotSupportedException();
            }
            var varName = NextAsString();
            var valueKeyword = Next().Skip();
            var value = valueKeyword.Run();
            
            Source.Scope.Add(varName, value);

            return null;
        }
    }
}