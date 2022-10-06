using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class MillTool3D : InitializerRoot, ITool
    {
        public Mesh _MeshPointer;
        public GameObject _Cube;
        public Material _CubeMaterial;
        private Transform _transform;
        public override void Initialize()
        {
            _transform = transform;
            Values = this.GetDefaultsOrCreate();
            Globals.Tool = this;
            Cube = _Cube;
            var meshFilter = Cube.GetComponent<MeshFilter>();
            MeshPointer ??= meshFilter.mesh;
            _MeshPointer = MeshPointer;
            Triangulator = new Triangulator(MeshPointer, false);
            meshFilter.mesh = Triangulator.CurrentMesh;
            Collider = Cube.GetComponent<MeshCollider>();
            Collider.sharedMesh = Triangulator.CurrentMesh;
            Vertices = Triangulator.CurrentMesh.vertices.ToList();
            Triangles = Triangulator.CurrentMesh.triangles.ToList();
            Colors = Vertices.Select(x => new Color(255, 255, 255, 255)).ToList();
            ToolConfig = Globals.ToolManager.Tools.FirstOrDefault();
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
            CubeMaterial = _CubeMaterial;
            MovementType = Globals.MethodManager.Get("Traverse").Index;
            OnConsumeStopCheck += async () =>
            {
                Position = Values.CurrentPath.Points.Last();
            };
            Self = GetComponent<Rigidbody>();
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
                PyroConsoleView.PushTextStatic("Traverse finished!",
                                               $"Total vertices cut: {cutResult.TotalVerticesCut.ToString()} ({((double) cutResult.TotalVerticesCut / Vertices.Count).Round().ToString(CultureInfo.InvariantCulture)}%)",
                                               $"Average time spent cutting: {cutResult.TotalTime.ToString(CultureInfo.InvariantCulture)}ms");
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
        public sbyte MovementType { get; set; }
        public Mesh MeshPointer { get; set; }
        public GameObject Cube { get; set; }
        public Material CubeMaterial { get; set; }
        public Rigidbody Self { get; set; }
        public Triangulator Triangulator { get; set; }
        public Vector3 Position {get => _transform.position; set => _transform.position = value; }
        public ToolValues Values { get; set; }
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
                transform.localScale = Vector3.one * (_toolConfig.Radius * 2);
            }
        }
        [SerializeField] 
        private ToolConfiguration _toolConfig;
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