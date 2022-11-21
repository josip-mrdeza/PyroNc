using System;

namespace Pyro.Nc.Exceptions;

public class CycleParameterException : Exception
{
    public CycleParameterException(string message) : base(message)
    {
        
    }
}