using System;

namespace Pyro.Nc.Exceptions;

public class CycleParameterException : NotifyException
{
    public CycleParameterException(string message) : base(message)
    {
        
    }
}