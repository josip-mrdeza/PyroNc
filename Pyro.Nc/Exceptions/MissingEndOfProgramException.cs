namespace Pyro.Nc.Exceptions;

public class MissingEndOfProgramException : NotifyException
{
    public MissingEndOfProgramException() : base("Missing M02/M30/M17 at the end of the file.")
    {
    }
}