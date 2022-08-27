using System;
using System.Threading;
using Pyro.Nc.Parsing;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class ToolValues
    {
        public ToolValues(ITool tool)
        {
            Storage = ValueStorage.CreateFromFile(tool).Result;
            Destination = new Target(new Vector3());
            IsAllowed = true;
            IsIncremental = false;
            IsImperial = false;
            Current = null;
            ExactStopCheck = false;
        }

        public ValueStorage Storage { get; set; }
        public Path CurrentPath { get; set; }
        public Target Destination { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsIncremental { get; set; }
        public bool IsImperial { get; set; }
        public bool IsMeasurementDirty { get; set; }
        public ICommand Current { get; set; }
        public bool ExactStopCheck { get; set; }
        public float SpindleSpeed { get; set; }
        public float SpindleSpeedLimit { get; set; }
        public float FeedRate { get; set; }
        public TimeSpan FastMoveTick = TimeSpan.FromMilliseconds(0.1d);
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
    }
}