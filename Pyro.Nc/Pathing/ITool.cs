using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation;
using UnityEngine;
using Path = Pyro.Nc.Pathing.Path;

namespace Pyro.Nc.Pathing
{
    public interface ITool
    {
        public Triangulator Triangulator { get; set; }
        public Vector3 Position { get; }
        public PObject Workpiece { get; set; }
        public ToolValues Values { get; set; }
        public event Func<Task> OnConsumeStopCheck;
        public event Func<ICommand, Vector3, Task> OnCollision;
        public Task InvokeOnConsumeStopCheck();
        public Task UseCommand(ICommand command, bool draw);
        public Task Traverse(Vector3[] points, bool draw);
        public Task Traverse(Line3D line, bool draw);
        public Task Traverse(Vector3 toPoint, LineTranslationSmoothness smoothness, bool draw);
        public Task Traverse(Circle3D circle, bool draw);
        public Task Traverse(Vector3 circleCenter, float circleRadius, bool draw);
        public Task Traverse(Vector3 circleCenter, float circleRadius, bool reverse, bool draw);
    }
}