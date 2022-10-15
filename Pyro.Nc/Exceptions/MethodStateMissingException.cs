using System;

namespace Pyro.Nc.Exceptions
{
    public class MethodStateMissingException : NotifyException
    {
        public MethodStateMissingException(string id) : base($"Method State '{id}' was missing from the MethodStateManager!")
        {
            
        }
    }
}