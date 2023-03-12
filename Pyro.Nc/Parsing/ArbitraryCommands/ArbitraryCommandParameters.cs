using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class ArbitraryCommandParameters : ICommandParameters
    {
        public Dictionary<string, float> Values { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, Func<float>> VarValues { get; set; }

        public float GetValue(string s)
        {
            if (Values.ContainsKey(s))
            {
                return Values[s];
            }
            return Single.NaN;
        }
        
        public float AddValue(string key, float val)
        {
            if (Values is null)
            {
                Values = new Dictionary<string, float>();
            }
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

        public CancellationToken Token => Globals.Tool.Values.TokenSource.Token;

        public LineTranslationSmoothness LineSmoothness
        {
            get => default;
            set => throw new NotSupportedException();  
        }

        public CircleSmoothness CircleSmoothness
        {
            get => default;
            set => throw new NotSupportedException();  
        }
    }
}