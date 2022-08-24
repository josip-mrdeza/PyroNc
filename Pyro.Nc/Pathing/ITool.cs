using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using UnityEngine;
using Path = Pyro.Nc.Pathing.Path;

namespace Pyro.Nc.Pathing
{
    public interface ITool
    {
        public ValueStorage Storage { get; set; }
        public Triangulator Triangulator { get; set; }
        public Vector3 Position { get; }
        public Path CurrentPath { get; set; }
        public PObject Workpiece { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsIncremental { get; set; }
        public ICommand Current { get; set; }
        public bool ExactStopCheck { get; set; }
        public event Func<Task> OnConsumeStopCheck;
        public event Func<ICommand, Vector3, Task> OnCollision;
        public CancellationTokenSource TokenSource { get; set; } 
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