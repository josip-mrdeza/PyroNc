using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class SpindleSpeedOverLimitException : Exception
    {
        public SpindleSpeedOverLimitException(ICommand command, float value, float limit) 
            : base($"[{command.GetType().Name}] Tried to set spindle speed to a value ({value}) exceeding it's limit ({limit}) set previously.")
        {
            
        } 
    }
}