using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
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
using Pyro.Threading;
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
            var color = new Color(1, 1, 1, 255f);
            Vertices = new List<Vector3>(Workpiece.Current.vertices);
            Triangles = new List<int>(Workpiece.Current.triangles);
            Workpiece.Current.MarkDynamic();
            Workpiece.Current.Optimize();
            Colors = Enumerable.Repeat(color, Vertices.Count).ToList();
            var tr = Cube.transform;
            /*Task.Run(() =>
            {
                var maxRadius = Globals.ToolManager.Tools.Max(x => x.Radius);
                var vertsAsSpan = Vertices.GetInternalArray().AsSpan();
                var caughtBlocks = new List<int>();
                var length = vertsAsSpan.Length;
                for (var i = 0; i < length; i++)
                {
                    var vertex = PyroDispatcher.ExecuteOnMain(t => tr.TransformPoint(Vertices[t]), i);
                    for (int j = 0; j < length; j++)
                    {
                        if (Vector3.Distance(vertex, vertsAsSpan[i]) <= maxRadius)
                        {
                            caughtBlocks.Add(i);
                        }
                    }
                    Sim3D.CachedBlocks.Add(i, caughtBlocks.ToArray());
                    caughtBlocks.Clear();
                }
                Globals.Console.Push($"Returned from thread {Thread.CurrentThread.ManagedThreadId.ToString()}, spawned in {nameof(MillTool3D)}.");
            });*/
            ToolConfig = await this.ChangeTool(0);
            var bounds = Collider.bounds;
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
            var copy = Values.Storage.TryGetCommand("TRANS").Copy() as Trans;
            var copy2 = Values.Storage.TryGetCommand("M05").Copy() as M05;
            var copy3 = Values.Storage.TryGetCommand("G40").Copy() as G40;
            var copy4 = Values.Storage.TryGetCommand("G54").Copy() as G54;
            var copy5 = Values.Storage.TryGetCommand("G90").Copy() as G90;
            EventSystem.AddAsyncSubscriber("ProgramEnd", ResetTrans(copy));
            EventSystem.AddAsyncSubscriber("ProgramEnd", StopSpindle(copy2));
            EventSystem.AddAsyncSubscriber("ProgramEnd", DisableG40(copy3));
            EventSystem.AddAsyncSubscriber("ProgramEnd", ResetG54(copy4));
            EventSystem.AddAsyncSubscriber("ProgramEnd", ResetG90(copy5));
            EventSystem.AddAsyncSubscriber("ProgramEnd", ResetTool());
            EventSystem.AddAsyncSubscriber("ProgramEnd", ResetSettings());
            EventSystem.AddAsyncSubscriber("RapidFeedError", ResolveRapidFeedCollision());
            Position = new Vector3(0, 50, 0);
        }

        private Func<Task> ResolveRapidFeedCollision()
        {
            return async () =>
            {
                await this.Pause();
                PushComment("A rapid feed error has occured whilst using the command '{0}'".Format(Values.Current), Color.red);
            };
        }

        private Func<Task> ResetSettings()
        {
            return () =>
            {
                Push("ProgramEnd->RESET SETTINGS");
                Values.FeedRate.Set(0f);
                Values.SpindleSpeed.Set(0f);
                Position = new Vector3(0, 50, 0);
                return Task.CompletedTask;
            };
        }

        private Func<Task> ResetTool()
        {
            return async () =>
            {
                Push("ProgramEnd->CHANGE TOOL TO DEFAULT(0)");
                this.Values.Current?.Expire();
                await this.ChangeTool(0);
            };
        }

        private Func<Task> ResetG90(G90 copy5)
        {
            return async () =>
            {
                Push("ProgramEnd->SET TO ABSOLUTE MODE");
                await copy5.Execute(false);
            };
        }

        private Func<Task> ResetG54(G54 copy4)
        {
            return async () =>
            {
                Push("ProgramEnd->RESET TEMPORARY REFERENCE POINT");
                await copy4.Execute(false);
            };
        }

        private Func<Task> DisableG40(G40 copy3)
        {
            return async () =>
            {
                Push("ProgramEnd->DISABLE (R) COMPENSATION");
                await copy3.Execute(false);
            };
        }

        private Func<Task> StopSpindle(M05 copy2)
        {
            return async () =>
            {
                Push("ProgramEnd->STOP SPINDLE");
                await copy2.Execute(false);
            };
        }

        private Func<Task> ResetTrans(Trans copy)
        {
            return async () =>
            {
                Push("ProgramEnd->RESET TRANS");
                await copy.Execute(false);
            };
        }

        private void FixedUpdate()
        {
            if (MovementType == -1 && Values.Current is
            {
                Family: Group.GCommand
            })
            {
                _contained = true;
                var pos = Position;
                var cutResult = this.CheckPositionForCut(Direction.FromVectors(pos, pos + Vector3.down), Values.Current.GetType() == typeof(G00));
                Push("Traverse finished!",
                                               "Total vertices cut: {0} ({1}%)".Format(cutResult.TotalVerticesCut, 
                                                   (((double) cutResult.TotalVerticesCut / Vertices.Count) * 100).Round()),
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