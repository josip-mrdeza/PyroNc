using System;
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
}