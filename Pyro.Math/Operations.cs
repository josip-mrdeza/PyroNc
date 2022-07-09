using System;
using System.Diagnostics.Contracts;

namespace Pyro.Math
{
    public static partial class Operations
    {
        public const float DegreeToRadian = (float) System.Math.PI / 180f;
        public const float CubeRootPower = (1f / 3);
        [Pure]
        public static float SquareRoot(this float x)
        {
            return (float) System.Math.Sqrt(x);
        }
        [Pure]
        public static float CubeRoot(this float x)
        {
            return (float) System.Math.Pow(x, CubeRootPower);
        }
        [Pure]
        public static float Squared(this float x)
        {
            return x * x;
        }
        [Pure]
        public static float Cubed(this float x)
        {
            return x * x * x;
        }
        [Pure]
        public static float Sin(this float angle)
        {
            return (float) System.Math.Sin(angle * DegreeToRadian);
        }
        
        [Pure]
        public static float Cos(this float angle)
        {
            return (float) System.Math.Cos(angle * DegreeToRadian);
        }
        
        [Pure]
        public static float Sin(this int angle)
        {
            return (float) System.Math.Sin(angle * DegreeToRadian);
        }
        
        [Pure]
        public static float Cos(this int angle)
        {
            return (float) System.Math.Cos(angle * DegreeToRadian);
        }

        [Pure]
        public static float Tan(this float angle)
        {
            return (float) System.Math.Tan(angle * DegreeToRadian);
        }
        
        [Pure]
        public static float Tan(this int angle)
        {
            return (float) System.Math.Tan(angle * DegreeToRadian);
        }
        
        /// <returns>An angle representing the 'x' of sine.</returns>
        [Pure]
        public static float ArcSin(this float x)
        {
            return (float) System.Math.Asin(x);
        }
        
        /// <returns>An angle representing the 'x' of cosine.</returns>
        [Pure]
        public static float ArcCos(this float x)
        {
            return (float) System.Math.Acos(x);
        }
        
        [Pure]
        public static float ArcTan(this float x)
        {
            return (float) System.Math.Atan(x);
        }
        
        /// <returns>An angle representing the 'x' of sine.</returns>
        [Pure]
        public static float ArcSin(this int x)
        {
            return (float) System.Math.Asin(x);
        }
        
        /// <returns>An angle representing the 'x' of cosine.</returns>
        [Pure]
        public static float ArcCos(this int x)
        {
            return (float) System.Math.Acos(x);
        }

        [Pure]
        public static float ArcTan(this int x)
        {
            return (float) System.Math.Atan(x);
        }
        
        public static float Average(params float[] nums)
        {
            float result = default(float);
            for (int i = 0; i < nums.Length; i++)
            {
                result += nums[i];
            }

            return result / nums.Length;
        }

        public static float Sum(params float[] nums)
        {
            float result = default(float);
            for (int i = 0; i < nums.Length; i++)
            {
                result += nums[i];
            }

            return result;
        } 
    }
}