using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class FeedRateOverLimitException : Exception
    {
        public FeedRateOverLimitException(ICommand command, float limit, float value)
            : base($"[{command.GetType().Name}] Tried to set feed rate to a value ({value}) exceeding it's limit ({limit}) set previously.")
        {
            
        } 
    }
}