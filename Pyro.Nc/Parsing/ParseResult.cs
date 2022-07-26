using System.Collections.Generic;

namespace Pyro.Nc.Parsing
{
    public struct ParseResult
    {
        public ElementType Type;
        public string Data;
        public GCode.Line Parent;

        public ParseResult(ElementType type, string data, GCode.Line parent)
        {
            Type = type;
            Data = data;
            Parent = parent;
        }
        
        public enum ElementType
        {
            Function,
            Parameter
        }
    }
}