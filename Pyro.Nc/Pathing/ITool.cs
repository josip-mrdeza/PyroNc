using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO.Events;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation;
using UnityEngine;
using Path = Pyro.Nc.Pathing.Path;

namespace Pyro.Nc.Pathing
{
    public interface ITool
    {
        public sbyte MovementType { get; set; }
        /// <summary>
        /// A reference to a mesh that is to be copied and used for cutting.
        /// </summary>
        public Mesh MeshPointer { get; set; }
        public GameObject Cube { get; set; }
        public Material CubeMaterial { get; set; }
        public Rigidbody Self { get; set; }
        public Triangulator Triangulator { get; set; }
        public Vector3 Position { get; set; }
        public ToolValues Values { get; set; }
        public PyroEventSystem EventSystem { get; set; }
        /// <summary>
        /// Stores the mesh's vertices.
        /// </summary>
        public List<Vector3> Vertices { get; set; }
        /// <summary>
        /// Stores the mesh's triangles.
        /// </summary>
        public List<int> Triangles { get; set; }
        /// <summary>
        /// Stores the mesh's vertex colors.
        /// </summary>
        public List<Color> Colors { get; set; }
        public MeshCollider Collider { get; set; }
        public ToolConfiguration ToolConfig { get;set; }
        public float MinX { get; }
        public float MinY { get; }
        public float MinZ { get; }
        public float MaxX { get; }
        public float MaxY { get; }
        public float MaxZ { get; }
        /// <summary>
        /// An event describing a Stop Check command event.
        /// </summary>
        public event Func<Task> OnConsumeStopCheck;
        /// <summary>
        /// Invokes the <see cref="OnConsumeStopCheck"/> event.
        /// </summary>
        /// <returns></returns>
        public Task InvokeOnConsumeStopCheck();
        /// <summary>
        /// Executes the command provided.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="draw">Whether to draw the path of the command, if available.</param>
        /// <returns>A task.</returns>
        public Task UseCommand(ICommand command, bool draw);
    }
}