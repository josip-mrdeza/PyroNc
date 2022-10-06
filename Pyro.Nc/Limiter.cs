using System.Globalization;

namespace Pyro.Nc
{
    public class Limiter
    {
        private float Value;
        public float UpperLimit { get; set; }

        public Limiter(float value, float upperLimit)
        {
            Value = value;
            UpperLimit = upperLimit;
        }

        public Limiter()
        {
        }
        
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

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }
}