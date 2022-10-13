using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class LinearInterpolationParameterMismatchException : ArgumentException
    {
        public LinearInterpolationParameterMismatchException(ICommand command)
            : base($"GCommand '{command.GetType().Name}' was invoked with all parameters set to 0, this is not valid in incremental mode.")
        {
            
        }
    }
}