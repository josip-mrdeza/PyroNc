namespace Pyro.Nc.Exceptions;

public class ForLoopNoEndException : NotifyException
{
    public ForLoopNoEndException(string message = "For loop does not contain a defined end (ENDFOR).") : base(message)
    {
    }
}