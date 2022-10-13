using System;
using System.Globalization;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class SpindleSpeedOverLimitException : Exception
    {
        public SpindleSpeedOverLimitException(ICommand command, float value, float limit) 
            : base("[{0}] Tried to set spindle speed to a value ({1}) exceeding it's limit ({2}) set previously"
                       .Format(command.GetType().Name, value, limit))
        {
            
        } 
    }
}