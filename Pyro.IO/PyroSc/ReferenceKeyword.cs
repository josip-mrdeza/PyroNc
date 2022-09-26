using System;
using System.Linq;

namespace Pyro.IO.PyroSc
{
    public class ReferenceKeyword : ExecutableKeyword
    {
        public ReferenceKeyword(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
        }

        private const string Accessor = ".";
        private const string ExpectedAssignmentOperator = "=";
        private bool _hasInitDoubleColon;
        private bool _hasInitAssignmentOp;
        private bool _hasDoubleColon;
        private bool _hasAssignmentOp;
        private string _className;

        public bool HasDoubleColon()
        {
            if (!_hasInitDoubleColon)
            {
                _hasDoubleColon = Next().Contents == Accessor;
                _hasInitDoubleColon = true;
            }

            return _hasDoubleColon;
        }
        
        public bool HasAssignmentOperator()
        {
            if (!_hasInitAssignmentOp)
            {
                _hasAssignmentOp = Elements.FirstOrDefault(k => k.Contents == ExpectedAssignmentOperator) != null;
                _hasInitAssignmentOp = true;
            }

            return _hasAssignmentOp;
        }

        protected string GetClassName()
        {
            if (!HasDoubleColon())
            {
                throw new NotSupportedException($"Missing '{Accessor}' accessor in ${Next().Contents}");
            }

            if (_className is null)
            {
                var formatted= NextAsString().Replace(Accessor, " ");
                _className = formatted.Split(' ')[1];
            }

            return _className;
        }
        
    }
}