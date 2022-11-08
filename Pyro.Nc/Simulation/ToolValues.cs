using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using Pyro.Nc.Configuration.Managers;
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
            Storage = ValueStorage.CreateFromFile(tool);
            ModalStorage = new Dictionary<string, ICommand>();
            Destination = new Target(new Vector3());
            SpindleSpeed = new Limiter(500, 3500);
            FeedRate = new Limiter(10, 350);
            TokenSource = new CancellationTokenSource();
            FastMoveTick = TimeSpan.FromMilliseconds(10f);
            WorkOffsets = new Vector3[4];
            //TransPosition = ReferencePointParser.MachineZeroPoint;
            IsAllowed = true;
            IsIncremental = false;
            IsImperial = false;
            Current = null;
            ExactStopCheck = true;
            IsMilling = true;
        }

        public ToolValues()
        {
            //Storage = ValueStorage.CreateFromFile(tool);
            ModalStorage = new Dictionary<string, ICommand>();
            Destination = new Target(new Vector3());
            SpindleSpeed = new Limiter(500, 3500);
            FeedRate = new Limiter(10, 350);
            FastMoveTick = TimeSpan.FromMilliseconds(10f);
            WorkOffsets = new Vector3[4];
            //TransPosition = ReferencePointParser.MachineZeroPoint;
            IsAllowed = true;
            IsIncremental = false;
            IsImperial = false;
            Current = null;
            ExactStopCheck = false;
            IsMilling = true; 
        }
        public ValueStorage Storage { get; set; }
        public Dictionary<string, ICommand> ModalStorage { get; set; }
        [JsonIgnore] public Path CurrentPath { get; set; }
        [JsonIgnore] public Target Destination { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsIncremental { get; set; }
        public bool IsImperial { get; set; }
        public bool IsMilling { get; set; }
        public ICommand Current { get; set; }
        public bool ExactStopCheck { get; set; }
        [JsonIgnore] public Vector3 TransPosition { get; set; }
        [JsonIgnore] public Vector3[] WorkOffsets { get; set; }
        public Limiter SpindleSpeed { get; set; }
        public Limiter FeedRate { get; set; }
        public float Radius => Globals.Tool.ToolConfig.Radius;
        public float SpindleSpeedLimit { set => SpindleSpeed.UpperLimit = value; } 
        public float FeedRateLimit { set => FeedRate.UpperLimit = value; }
        public TimeSpan FastMoveTick { get; set; }
        [JsonIgnore] public CancellationTokenSource TokenSource { get; set; }
    }
}
