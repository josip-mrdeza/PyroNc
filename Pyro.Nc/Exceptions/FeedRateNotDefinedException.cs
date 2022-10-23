namespace Pyro.Nc.Exceptions
{
    public class FeedRateNotDefinedException : NotifyException
    {
        public FeedRateNotDefinedException() : base("Can't move tool with no feed rate defined!")
        {
        }
        
        public FeedRateNotDefinedException(string message) : base(message)
        {
        }
    }
}