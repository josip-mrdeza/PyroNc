namespace Pyro.Nc
{
    public class Limiter
    {
        private float Value;
        public float UpperLimit;

        public void Set(float val)
        {
            if (val > UpperLimit)
            {
                Value = UpperLimit;
            }
            else if (val < 0)
            {
                Value = 0;
            }
            else
            {
                Value = val;
            }
        }

        public float Get()
        {
            return Value;
        }
        
        public static implicit operator float(Limiter limiter)
        {
            return limiter.Value;
        }
    }
}