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
        public GameObject Cube { get; set; }
        public GameObject Temp { get; set; }
        public sbyte MovementType { get; set; }
        /// <summary>
        /// A reference to a mesh that is to be copied and used for cutting.
        /// </summary>
        public Rigidbody Self { get; set; }
        public Vector3 Position { get; set; }
        public ToolValues Values { get; set; }
        public PyroEventSystem EventSystem { get; }
        public WorkpieceController Workpiece { get; set; }
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
        public LineRenderer LineRenderer { get; set; }
        public ToolConfiguration ToolConfig { get;set; }
        public Dictionary<int, int[][]> VertexToTriangleMapping { get; }
        public Dictionary<ValueTuple<Vector3, Vector3>, List<Algorithms.VertexMap>> VertexHashmap { get; }
        public float MinX { get; set; }
        public float MinY { get; set; }
        public float MinZ { get; set; }
        public float MaxX { get; set; }
        public float MaxY { get; set; }
        public float MaxZ { get; set; }
        /// <summary>
        /// An event describing a Stop Check command event.
        /// </summary>
        public event Func<Task> OnConsumeStopCheck;
        public event Action<Vector3> OnPositionChanged;
        public event Action<Vector3> OnTransChanged;
        public event Action<float> OnFeedRateChanged;
        public event Action<float> OnSpindleSpeedChanged;
        public event Action<ToolConfiguration> OnToolChanged;
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