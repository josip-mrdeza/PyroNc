using System.Collections.Generic;
using System.Linq;

namespace Pyro.Nc.Parsing
{
    public class GParser
    {
        public GCode Code;
        public Rule[] Rules;
        public GParser()
        {
            
        }
        
        public GParser(GCode code)
        {
            Code = code;
        }
        
        public GParser(GCode code, Rule[] rules)
        {
            Code = code;
            Rules = rules;
        }

        public IEnumerable<string[]> Split()
        {
            IEnumerable<string[]> arr = Code.Select(x => x.Contents.Split(_splitter));

            return arr;
        }

        public IEnumerable<ParseResult> Order()
        {
            return null;
        }
        

        
        
        private static readonly char[] _splitter = new char[]
        {
            ' '
        };
    }
}