using System.Collections.Generic;
using Pyro.Math;
using Pyro.Math.Geometry;

namespace Pyro.Nc.Parsing.GCommands
{
    public class GCommandParameters : ICommandParameters
    {
        public Dictionary<string, float> Values { get; set; }
        public float GetValue(string s)
        {
            return Values[s];
        }

        public LineTranslationSmoothness LineSmoothness { get; set; }
        public CircleSmoothness CircleSmoothness { get; set; }

        public GCommandParameters(float x, float y, float z, LineTranslationSmoothness smoothness = LineTranslationSmoothness.Crude)
        {
            Values = new Dictionary<string, float>()
            {
                {"X", x},
                {"Y", y},
                {"Z", z},
                {"I", float.NaN},
                {"J", float.NaN}
            };
            LineSmoothness = smoothness;
        }
        
        public GCommandParameters(float x, float y, float z, float radius, CircleSmoothness smoothness)
        {
            Values = new Dictionary<string, float>()
            {
                {"X", x},
                {"Y", y},
                {"Z", z},
                {"I", float.NaN},
                {"J", float.NaN},
                {"K", float.NaN},
                {"R", radius}
            };
            CircleSmoothness = smoothness;
        }
        
        public float X => GetValue("X");
        public float Y => GetValue("Y");
        public float Z => GetValue("Z");
        
        public float I => GetValue("I");
        public float J => GetValue("J");
        public float R => GetValue("R");
    }
}