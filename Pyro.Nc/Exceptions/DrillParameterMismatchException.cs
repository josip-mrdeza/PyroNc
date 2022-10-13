using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class DrillParameterMismatchException : LinearInterpolationParameterMismatchException
    {
        public DrillParameterMismatchException(ICommand drillCommand) : base(drillCommand)
        {
            
        }
    }
}