using System;

namespace Pyro.Nc.Exceptions;

public class NoSubroutineIDProvidedException : NotifyException
{
    public NoSubroutineIDProvidedException(string message, bool asWarning = true) : base(message, asWarning)
    {
    }
}