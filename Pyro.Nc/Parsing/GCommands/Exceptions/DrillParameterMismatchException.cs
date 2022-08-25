using System;

namespace Pyro.Nc.Parsing.GCommands.Exceptions
{
    public class DrillParameterMismatchException : LinearInterpolationParameterMismatchException
    {
        public DrillParameterMismatchException(ICommand drillCommand) : base(drillCommand)
        {
            
        }
    }
}