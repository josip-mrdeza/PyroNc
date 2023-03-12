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
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Nc.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation
{
    [Obsolete]
    public class MillTool3D : InitializerRoot
    {
        private Transform _transform;
        public Dictionary<int, int[][]> VertexToTriangleMapping { get; private set; }
        public Dictionary<ValueTuple<Vector3, Vector3>, List<Algorithms.VertexMap>> VertexHashmap { get; private set; }
        public LineRenderer Lr;
        public float increment;

        public override void Initialize()
        {
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
            //Values = this.GetDefaultsOrCreate();
            //Globals.Tool = this;
            Collider = Workpiece.GetComponent<MeshCollider>();
            //await Workpiece.InitializeAsync();
            //Collider.sharedMesh = Workpiece.Current;
            var color = new Color(1, 1, 1, 255f);
            //Vertices = new List<Vector3>(Workpiece.Current.vertices);
            //Triangles = new List<int>(Workpiece.Current.triangles);
            VertexToTriangleMapping = new Dictionary<int, int[][]>(Vertices.Count);
            //Workpiece.Current.MarkDynamic();
            //Workpiece.Current.Optimize();
            Stopwatch stopwatch = Stopwatch.StartNew();
            //VertexHashmap = Workpiece.Current.GenerateVertexHashmap(increment);                                      
            var num = VertexHashmap.Sum(x => x.Value.Count);
            stopwatch.Stop();
            Globals.Console.Push($"Vertex hashmap calculated in {stopwatch.Elapsed.TotalMilliseconds.Round(1)}ms [{num}v's]");
            foreach (var vhv in VertexHashmap)
            {
                var val1 = vhv.Key.Item1;
                var val2 = vhv.Key.Item2;
                    
                Debug.DrawLine(val1, new Vector3(val2.x, val1.y, val2.z), Color.red, 10000);
                for (var i = 1; i < vhv.Value.Count; i++)
                {
                    Debug.DrawLine(vhv.Value[i-1].Vertex, vhv.Value[i].Vertex, Color.green, 10000);
                }
            }
            //ToolConfig = await this.ChangeTool(0);                          
            MovementType = Globals.MethodManager.Get("Traverse").Index;
            Self = GetComponent<Rigidbody>();
            //Self.maxAngularVelocity = Values.SpindleSpeed.UpperLimit;
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
            Position = new Vector3(-50, 100, -50);
            LineRenderer = Lr;

        }

        private void RunCacheIndexing()
        {
            var vertLength = Vertices.Count;
            var trigLength = Triangles.Count;
            var cache = LocalRoaming.OpenOrCreate("PyroNc\\Cache");
            if (cache.Exists("CachedVertexMapping.map"))
            {
                VertexToTriangleMapping = cache.ReadFileAs<Dictionary<int, int[][]>>("CachedVertexMapping.map");
                return;
            }
            Parallel.For((long)0, vertLength, new ParallelOptions(){MaxDegreeOfParallelism =4 },
                         () =>
                         {
                             return new List<int[]>();
                         },
                         (i, _, list) =>
                         {
                             for (int t = 0; t < trigLength; t += 3)
                             {
                                 if (i == t || i == t + 1 || i == t + 2)
                                 {
                                     list.Add(new[]
                                     {
                                         t,
                                         t + 1,
                                         t + 2
                                     });
                                     PDispatcher.ExecuteOnMain(() =>
                                     {
                                         LoadingScreenView.Instance
                                                          .SetAdditionalText($"{t}/{trigLength}");
                                     });
                                 }
                             }

                             VertexToTriangleMapping.Add((int)i, list.ToArray());
                             PDispatcher.ExecuteOnMain(
                                 () => LoadingScreenView.Instance.SetText(
                                     $"Cached {list.Count.ToString()} triangles for vertex {i}"));
                             list.Clear();

                             return list;
                         }, list =>
                         {
                             
                         });
            for (int i = 0; i < vertLength; i++)
            {
                if (!VertexToTriangleMapping.ContainsKey(i))
                {
                    VertexToTriangleMapping.Add(i, Array.Empty<int[]>());
                }
            }
            Debug.Log($"Total cached: {VertexToTriangleMapping.Count}/{vertLength}");
            cache.AddFile("CachedVertexMapping.map", VertexToTriangleMapping);
        }

        private Func<Task> ResolveRapidFeedCollision()
        {
            return async () =>
            {
                //await this.Pause();
                //PushComment("A rapid feed error has occured whilst using the command '{0}'".Format(Values.Current), Color.red);
            };
        }

        private Func<Task> ResetSettings()
        {
            return () =>
            {
                //Push("ProgramEnd->RESET SETTINGS");
                //Values.FeedRate.Set(0f);
                //Values.SpindleSpeed.Set(0f);
                Position = new Vector3(-50, 100, -50);
                MCALL.ClearSubroutine();
                DEF.ClearVariableMap();
                return Task.CompletedTask;
            };
        }

        private Func<Task> ResetTool()
        {
            return async () =>
            {
                //Push("ProgramEnd->CHANGE TOOL TO DEFAULT(0)");
                //this.Values.Current?.Expire();
                //await this.ChangeTool(0);
            };
        }

        private Func<Task> ResetG90(G90 copy5)
        {
            return async () =>
            {
                //Push("ProgramEnd->SET TO ABSOLUTE MODE");
                await copy5.Execute(false);
            };
        }

        private Func<Task> ResetG54(G54 copy4)
        {
            return async () =>
            {
                //Push("ProgramEnd->RESET TEMPORARY REFERENCE POINT");
                await copy4.Execute(false);
            };
        }

        private Func<Task> DisableG40(G40 copy3)
        {
            return async () =>
            {
                //Push("ProgramEnd->DISABLE (R) COMPENSATION");
                await copy3.Execute(false);
            };
        }

        private Func<Task> StopSpindle(M05 copy2)
        {
            return async () =>
            {
                //Push("ProgramEnd->STOP SPINDLE");
                await copy2.Execute(false);
            };
        }

        private Func<Task> ResetTrans(Trans copy)
        {
            return async () =>
            {
                //Push("ProgramEnd->RESET TRANS");
                await copy.Execute(false);
            };
        }

        private void FixedUpdate()
        {
            return;
            /*if (MovementType == -1 && Values.Current is
            {
                Family: Group.GCommand
            })
            {
                _contained = true;
                var pos = Position;
                //var cutResult = this.CheckPositionForCut(Direction.FromVectors(pos, pos + Vector3.down), Values.Current.GetType() == typeof(G00));
                // Push("Traverse finished!",
                //                                "Total vertices cut: {0} ({1}%)".Format(cutResult.TotalVerticesCut, 
                //                                    (((double) cutResult.TotalVerticesCut / Vertices.Count) * 100).Round()),
                //                                "Average time spent cutting: {0}ms"
                //                                    .Format(cutResult.TotalTime));
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
            }  */
        }

        private bool _contained;
        public GameObject Cube { get; set; }
        public GameObject Temp { get; set; }
        public sbyte MovementType { get; set; }
        public Rigidbody Self { get; set; }
        public Vector3 Position
        {
            get => _transform.position;
            set
            {
                _transform.position = value;
                OnPositionChanged?.Invoke(value);
            }
        }

        public ToolValues Values { get; set; }
        public PyroEventSystem EventSystem { get; set; }
        public WorkpieceControl Workpiece { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<Color> Colors { get; set; }
        public MeshCollider Collider { get; set; }
        public LineRenderer LineRenderer { get; set; }

        public ToolConfiguration ToolConfig
        {
            get => _toolConfig;
            set
            {
                _toolConfig = value;
                //Resources.UnloadAsset(meshFilter.mesh);
                var str = "Tools/{0}".Format(_toolConfig.Id);
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
        public event Action<Vector3> OnPositionChanged;
        public event Action<Vector3> OnTransChanged;
        public event Action<float> OnFeedRateChanged;
        public event Action<float> OnSpindleSpeedChanged;
        public event Action<ToolConfiguration> OnToolChanged;

        public void InvokeOnPositionChanged(Vector3 v)
        {
            OnPositionChanged?.Invoke(v);
        }
        
        public void InvokeOnTransChanged(Vector3 v)
        {
            OnTransChanged?.Invoke(v);
        }

        public void InvokeOnFeedRateChanged(float feed)
        {
            OnFeedRateChanged?.Invoke(feed);
        }

        public void InvokeOnSpindleSpeedChanged(float rpm)
        {
            OnSpindleSpeedChanged?.Invoke(rpm);
        }

        public void InvokeOnToolChanged(ToolConfiguration config)
        {
            OnToolChanged?.Invoke(config);
        }

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