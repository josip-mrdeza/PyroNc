using System;
using System.Collections.Generic;
using System.Threading;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class ToolValues
    {
        public ToolValues(ITool tool)
        {
            Storage = ValueStorage.CreateFromFile(tool).Result;
            ModalStorage = new Dictionary<string, ICommand>();
            Destination = new Target(new Vector3());
            SpindleSpeed = new Limiter();
            FeedRate = new Limiter();
            IsAllowed = true;
            IsIncremental = false;
            IsImperial = false;
            Current = null;
            ExactStopCheck = false;
            IsMilling = true;
        }

        public ValueStorage Storage { get; set; }
        public Dictionary<string, ICommand> ModalStorage { get; }
        public Path CurrentPath { get; set; }
        public Target Destination { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsIncremental { get; set; }
        public bool IsImperial { get; set; }
        public bool IsMilling { get; set; }
        public ICommand Current { get; set; }
        public bool ExactStopCheck { get; set; }
        public Limiter SpindleSpeed { get; set; }
        public Limiter FeedRate { get; set; }
        public float SpindleSpeedLimit
        {
            set => SpindleSpeed.UpperLimit = value;
        }
        public float FeedRateLimit
        {
            set => FeedRate.UpperLimit = value;
        }
        public TimeSpan FastMoveTick = TimeSpan.FromMilliseconds(0.1d);
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
    }
}
