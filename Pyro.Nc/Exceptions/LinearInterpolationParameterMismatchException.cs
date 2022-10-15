using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class LinearInterpolationParameterMismatchException : NotifyException
    {
        public LinearInterpolationParameterMismatchException(ICommand command)
            : base("Command '{0}' was invoked with all parameters set to 0, this is not valid in incremental mode."
                       .Format(command.GetType().Name))
        {
            
        }
    }
}