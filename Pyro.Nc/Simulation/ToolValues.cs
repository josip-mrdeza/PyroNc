using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class ToolValues
    {
        public ToolValues(ToolBase toolBase)
        {
            Storage = ValueStorage.CreateFromFile(toolBase);
            Destination = new Target(new Vector3());
            TokenSource = new CancellationTokenSource();
            FastMoveTick = TimeSpan.FromMilliseconds(1f);
            ExactStopCheck = true;
        }

        public ToolValues()
        {
            Destination = new Target(new Vector3());
            FastMoveTick = TimeSpan.FromMilliseconds(1f);
            ExactStopCheck = false;
        }
        public ValueStorage Storage { get; set; }
        [JsonIgnore] public Path CurrentPath { get; set; }
        [JsonIgnore] public Target Destination { get; set; }
        public bool ExactStopCheck { get; set; }
        public float Radius => Globals.Tool is null ? 0 : Globals.Tool.ToolConfig.Radius;
        public TimeSpan FastMoveTick { get; set; }
        [JsonIgnore] public CancellationTokenSource TokenSource { get; set; }
    }
}
