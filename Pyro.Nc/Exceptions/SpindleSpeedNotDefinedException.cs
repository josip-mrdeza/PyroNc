namespace Pyro.Nc.Exceptions
{
    public class SpindleSpeedNotDefinedException : NotifyException
    {
        public SpindleSpeedNotDefinedException() : base("Can't cut while the spindle speed is set to 0!")
        {
        }
        
        public SpindleSpeedNotDefinedException(string message) : base(message)
        {
        }
    }
}