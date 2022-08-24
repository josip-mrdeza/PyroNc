using System.Collections.Generic;
using System.Threading;
using Pyro.Math;
using Pyro.Math.Geometry;

namespace Pyro.Nc.Parsing.GCommands
{
    public interface ICommandParameters
    {
        public Dictionary<string, float> Values { get; set; }

        public float GetValue(string s);
        public float AddValue(string key, float val);
        public CancellationToken Token { get; set; }
        public LineTranslationSmoothness LineSmoothness { get; set; }
        public CircleSmoothness CircleSmoothness { get; set; }
    }
}