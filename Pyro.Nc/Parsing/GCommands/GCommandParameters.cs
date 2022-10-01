using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public float AddValue(string key, float val)
        {
            if (Values.ContainsKey(key))
            {
                Values[key] = val;

                return val;
            }
            Values.Add(key, val);

            return val;
        }
        
        public void SwitchToImperial()
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var key = Values.Keys.ElementAt(i); 
                Values[key] *= 2.54f;
            }
        }

        public void SwitchToMetric()
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var key = Values.Keys.ElementAt(i); 
                Values[key] /= 2.54f;
            }
        }

        public CancellationToken Token { get; set; }
        public LineTranslationSmoothness LineSmoothness { get; set; }
        public CircleSmoothness CircleSmoothness { get; set; }

        public GCommandParameters(float x, float y, float z, LineTranslationSmoothness smoothness = LineTranslationSmoothness.Standard)
        {
            Values = new Dictionary<string, float>()
            {
                {"X", x},
                {"Y", y},
                {"Z", z},
                {"I", 0},
                {"J", 0},
                {"K", 0}
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
                {"I", 0},
                {"J", 0},
                {"K", 0},
                {"K", 0},
                {"R", radius}
            };
            CircleSmoothness = smoothness;
        }
        
        public float X => GetValue("X");
        public float Y => GetValue("Y");
        public float Z => GetValue("Z");
        
        public float I => GetValue("I");
        public float J => GetValue("J");
        public float K => GetValue("K");
        public float R => GetValue("R");
    }
}