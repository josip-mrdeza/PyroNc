using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing.GCommands;

namespace Pyro.Nc.Parsing.MCommands
{
    public class MCommandParameters : ICommandParameters
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

        public LineTranslationSmoothness LineSmoothness
        {
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException();  
        }

        public CircleSmoothness CircleSmoothness
        {
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException();  
        }
    }
}