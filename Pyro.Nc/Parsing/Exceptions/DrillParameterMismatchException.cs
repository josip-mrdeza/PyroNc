using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class DrillParameterMismatchException : LinearInterpolationParameterMismatchException
    {
        public DrillParameterMismatchException(ICommand drillCommand) : base(drillCommand)
        {
            
        }
    }
}