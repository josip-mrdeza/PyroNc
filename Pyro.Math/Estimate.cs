using System;
using System.Globalization;
using Pyro.Math.Geometry;

namespace Pyro.Math
{
    public static class Estimate
    {
        public const float STDev = 3.335912f;
        public static float Velocity(float delay, LineTranslationSmoothness translationSmoothness)
        {
            var pure = (int) translationSmoothness * delay;
            var dts = pure * STDev;

            return dts;
        }
        public static LineTranslationSmoothness LineSmoothnessForVelocity(float delay, float velocity)
        {
            //var dts = translationSmoothness * delay * STDev;
            //velocity = trs * delay * STDev;
            var translationSmoothness = velocity / (delay * STDev);
            var dts = (float) System.Math.Round(translationSmoothness, 0);

            if (dts is > ((int) LineTranslationSmoothness.Rough * 0.75f) and < ((int) LineTranslationSmoothness.Rough * 1.25f))
            {
                return LineTranslationSmoothness.Rough;
            }
            else if (dts is > ((int) LineTranslationSmoothness.Crude * 0.75f) and < ((int) LineTranslationSmoothness.Crude * 1.25f))
            {
                return LineTranslationSmoothness.Crude;
            }
            else if (dts is > ((int) LineTranslationSmoothness.Standard * 0.75f) and < ((int) LineTranslationSmoothness.Standard * 1.25f))
            {
                return LineTranslationSmoothness.Standard;
            }
            else if (dts is > ((int) LineTranslationSmoothness.Fine * 0.5f) and < ((int) LineTranslationSmoothness.Fine * 1.5f))
            {
                return LineTranslationSmoothness.Fine;
            }
            else if (dts is > ((int) LineTranslationSmoothness.Perfect * 0.9f))
            {
                return LineTranslationSmoothness.Perfect;
            }

            return 0;
        }
    }
}