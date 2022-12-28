using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PyroNetServerIntermediateLibrary
{
    public class Session
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public TimeSpan LastUpdated { get; set; }
        public List<string> Users { get; set; } = new List<string>();
        public Stopwatch TimeSinceUpdate;

        public void Update()
        {
            LastUpdated = GetTimeSinceUpdate();
            TimeSinceUpdate.Restart();
        }

        public void UpdateTimeOnly()
        {
            LastUpdated = GetTimeSinceUpdate();
        }

        public TimeSpan GetTimeSinceUpdate()
        {
            return TimeSinceUpdate.Elapsed;
        }

        public Session Copy()
        {
            return (Session) MemberwiseClone();
        }
    }
}