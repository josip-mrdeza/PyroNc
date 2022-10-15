using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class FeedRateOverLimitException : NotifyException
    {
        public FeedRateOverLimitException(ICommand command, float value, float limit)
            : base("[{0}] Tried to set feed rate to a value ({1}) exceeding it's limit ({2}) set previously."
                       .Format(command.GetType().Name, value, limit))
        {
            
        } 
    }
}