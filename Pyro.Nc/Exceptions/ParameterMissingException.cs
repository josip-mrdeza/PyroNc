namespace Pyro.Nc.Exceptions;

public class ParameterMissingException : NotifyException
{
    public ParameterMissingException(string parameterName) : base($"[{CurrentContext.GetType().Name}]: Missing required parameter '{parameterName}'!", false)
    {
    }
}