using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class RuleParseException : NotifyException
    {
        public RuleParseException(Rule rule) : base("The value passed to the rule '{0}' was deemed non-acceptable."
                                                        .Format(rule.Name))
        {
            
        }
        
        public RuleParseException(string message) : base(message)
        {
            
        }
    }
    
    public class RuleParseListException : RuleParseException
    {
        public RuleParseListException(string message, IEnumerable<object> data) : base("{0}{1}{2}".Format(message, '\n', string.Join(", \n", data)))
        {
            
        }
    }
}