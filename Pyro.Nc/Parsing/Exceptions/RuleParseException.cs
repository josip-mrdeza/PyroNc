using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class RuleParseException : Exception
    {
        public RuleParseException(Rule rule) : base($"The value passed to the rule '{rule.Name}' was deemed non-acceptable.")
        {
            
        }
        
        public RuleParseException(string message) : base(message)
        {
            
        }
    }
}