namespace Pyro.Nc.Exceptions
{
    public class ToolNotDefinedException : NotifyException
    {
        public ToolNotDefinedException() : base("Cannot start command, tool is not defined.")
        {
        }
        
        public ToolNotDefinedException(string message) : base(message)
        {
        }
    }
}