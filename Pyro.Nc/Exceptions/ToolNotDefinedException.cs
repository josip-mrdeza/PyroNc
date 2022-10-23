namespace Pyro.Nc.Exceptions
{
    public class ToolNotDefinedException : NotifyException
    {
        public ToolNotDefinedException() : base("Can't move while the tool is not defined.")
        {
        }
        
        public ToolNotDefinedException(string message) : base(message)
        {
        }
    }
}