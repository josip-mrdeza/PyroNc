using System;

namespace Pyro.Nc.Exceptions
{
    public class ToolMissingException : NotifyException
    {
        public ToolMissingException(int toolIndex)
            : base("Tool 'T{0}' is missing."
                       .Format(toolIndex))
        {
            
        }
    }
}