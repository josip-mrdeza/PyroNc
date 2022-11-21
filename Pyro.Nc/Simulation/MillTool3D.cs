using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pyro.Nc.Simulation
{
    public class MillTool3D : InitializerRoot, ITool
    {
        private Transform _transform;
        public override void Initialize()
        {
            throw new NotImplementedException("The async version of this method is implemented and used.");
        }

        public override async Task InitializeAsync()
        {  
            //const float c = 0.18823529411764705882352941176471f;
            Temp = GameObject.FindWithTag("temp");
            _transform = transform;
            EventSystem = new PyroEventSystem();
            Workpiece = Globals.Workpiece;
            Cube = Workpiece.gameObject;
            var customAssemblyManager = new CustomAssemblyManager();
            await customAssemblyManager.InitAsync();
            Startup.Managers.Add(customAssemblyManager);
            Values = this.GetDefaultsOrCreate();
            Globals.Tool = this;
            Collider = Workpiece.GetComponent<MeshCollider>();
            await Workpiece.InitializeAsync();
            //Collider.sharedMesh = Workpiece.Current;
            var color = new Color(1, 1, 1, 1f);
            Vertices = new List<Vector3>(Workpiece.Current.vertices);
            Triangles = new List<int>(Workpiece.Current.triangles);
            Colors = Enumerable.Repeat(color, Vertices.Count).ToList();
            ToolConfig = await this.ChangeTool(0);
            var bounds = Collider.bounds;
            var tr = Cube.transform;
            var max = tr.TransformVector(bounds.max);
            MaxX = max.x;
            MaxY = max.y;
            MaxZ = max.z;
            var min = tr.TransformVector(bounds.min);
            MinX = min.x;
            MinY = min.y;
            MinZ = min.z;
            MovementType = Globals.MethodManager.Get("Traverse").Index;
            Self = GetComponent<Rigidbody>();
            Self.maxAngularVelocity = Values.SpindleSpeed.UpperLimit;
            OnConsumeStopCheck += () =>
            {
                Position = Values.CurrentPath.Points.Last();
                return Task.CompletedTask;
            };
            var m0 = Values.Storage.TryGetCommand("M0") as M00;
            var copy = Values.Storage.TryGetCommand("TRANS").Copy() as Trans;
            var copy2 = Values.Storage.TryGetCommand("M05").Copy() as M05;
            var copy3 = Values.Storage.TryGetCommand("G40").Copy() as G40;
            var copy4 = Values.Storage.TryGetCommand("G54").Copy() as G54;
            var copy5 = Values.Storage.TryGetCommand("G90").Copy() as G90;
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->RESET TRANS");
                await copy.Execute(false);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->STOP SPINDLE");
                await copy2.Execute(false);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->DISABLE (R) COMPENSATION");
                await copy3.Execute(false);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->RESET TEMPORARY REFERENCE POINT");
                await copy4.Execute(false);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->SET TO ABSOLUTE MODE");
                await copy5.Execute(false);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", async () =>
            {
                Push("ProgramEnd->CHANGE TOOL TO DEFAULT(0)");
                this.Values.Current.Expire();
                await this.ChangeTool(0);
            });
            EventSystem.AddAsyncSubscriber("ProgramEnd", () =>
            {
                Push("ProgramEnd->RESET SETTINGS");
                Values.FeedRate.Set(0f);
                Values.SpindleSpeed.Set(0f);
                Position = new Vector3(0, 50, 0);
                return Task.CompletedTask;
            });
            EventSystem.AddAsyncSubscriber("RapidFeedError", async () =>
            {
                await this.Pause();
                PushComment("A rapid feed error has occured whilst using the command '{0}'".Format(Values.Current), Color.red);
            });
            Position = new Vector3(0, 50, 0);
        }

        private async void FixedUpdate()
        {
            if (MovementType == -1 && Values.Current is
            {
                Family: Group.GCommand
            })
            {
                _contained = true;
                var pos = Position;
                var cutResult = await this.CheckPositionForCut(Direction.FromVectors(pos, pos + Vector3.down), Values.Current.GetType() == typeof(G00));
                Push("Traverse finished!",
                                               "Total vertices cut: {0} ({1}%)".Format(cutResult.TotalVerticesCut, 
                                                   ((double) cutResult.TotalVerticesCut / Vertices.Count).Round()),
                                               "Average time spent cutting: {0}ms"
                                                   .Format(cutResult.TotalTime));
            }
            else
            {
                if (_contained)
                {
                    //Debug.Log($"Before: {Self.velocity}");
                    Self.velocity = Vector3.zero;
                    _contained = false;
                    //Debug.Log($"Afterwards: {Self.velocity}");
                }   
            }
        }

        private bool _contained;
        public GameObject Cube { get; set; }
        public GameObject Temp { get; set; }
        public sbyte MovementType { get; set; }
        public Rigidbody Self { get; set; }
        public Vector3 Position {get => _transform.position; set => _transform.position = value; }
        public ToolValues Values { get; set; }
        public PyroEventSystem EventSystem { get; set; }
        public WorkpieceController Workpiece { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<Color> Colors { get; set; }
        public MeshCollider Collider { get; set; }
        public ToolConfiguration ToolConfig
        {
            get => _toolConfig;
            set
            {
                _toolConfig = value;
                //Resources.UnloadAsset(meshFilter.mesh);
                var str = "Tools/{0}".Format(_toolConfig.Name);
                toolfilter.mesh = Resources.Load<Mesh>(str);
                //transform.localScale = Vector3.one * (_toolConfig.Radius * 2);
            }
        }                                                               
        private ToolConfiguration _toolConfig;

        public MeshFilter toolfilter;
        public float MinX { get; set; }
        public float MinY { get; set; } 
        public float MinZ { get; set; }
        public float MaxX { get; set; }
        public float MaxY { get; set; }
        public float MaxZ { get; set; }
        public event Func<Task> OnConsumeStopCheck;

        public async Task InvokeOnConsumeStopCheck()
        {
            if (OnConsumeStopCheck is not null)
            {
                await OnConsumeStopCheck();
            }
        }

        public async Task UseCommand(ICommand command, bool draw)
        {
            await command.ExecuteFinal(draw);
        }
    }
}