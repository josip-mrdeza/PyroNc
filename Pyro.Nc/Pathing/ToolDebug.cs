using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pyro.Nc.Pathing
{
    public class ToolDebug : InitializerRoot, ITool
    {
        public Mesh _MeshPointer;
        public GameObject _Cube;
        public sbyte MovementType { get; set; }
        public Mesh MeshPointer { get; set; }
        public GameObject Cube { get; set; }
        public Material CubeMaterial { get; set; }
        public Rigidbody Self { get; set; }
        public Triangulator Triangulator { get; set; }
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        } 
        public ToolValues Values { get; set; }
        public event Func<Task> OnConsumeStopCheck;
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<Color> Colors { get; set; }
        
        public MeshCollider Collider { get; set; }
        public ToolConfiguration ToolConfig { get; set; }
        public float MinX { get; private set;}
        public float MinY { get; private set;}
        public float MinZ { get; private set;}
        public float MaxX { get; private set;}
        public float MaxY { get; private set;}
        public float MaxZ { get; private set; }

        public async Task InvokeOnConsumeStopCheck()
        {
            if (OnConsumeStopCheck != null)
            {
                await OnConsumeStopCheck();
            }
        }

        public override void Initialize()
        {
            Values = new ToolValues(this);
            Globals.Tool = this;
            Cube = _Cube;
            var meshFilter = Cube.GetComponent<MeshFilter>();
            Collider = Cube.GetComponent<MeshCollider>();
            MeshPointer ??= meshFilter.mesh;
            Triangulator = new Triangulator(MeshPointer, false);
            meshFilter.mesh = Triangulator.CurrentMesh;
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
        }
        /// <summary>
        /// Executes the provided command.
        /// </summary>
        public async Task UseCommand(ICommand command, bool draw)
        {
            await command.ExecuteFinal(draw);
        }
    }
}
