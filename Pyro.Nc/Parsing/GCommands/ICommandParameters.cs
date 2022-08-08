using System.Collections.Generic;
using Pyro.Math;
using Pyro.Math.Geometry;

namespace Pyro.Nc.Parsing.GCommands
{
    public interface ICommandParameters
    {
        public Dictionary<string, float> Values { get; set; }

        public float GetValue(string s);
        public float AddValue(string key, float val);
        
        public LineTranslationSmoothness LineSmoothness { get; set; }
        public CircleSmoothness CircleSmoothness { get; set; }
    }
}