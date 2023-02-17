using System;
using System.Globalization;
using UnityEngine;

namespace Pyro.Nc
{
    [Serializable]
    public class Limiter
    {
        [SerializeField] private float Value;
        [SerializeField] private float _upperLimit;

        public float UpperLimit
        {
            get => _upperLimit;
            set => _upperLimit = value;
        }

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

        public void SetUpperValue(float upper)
        {
            _upperLimit = upper;
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