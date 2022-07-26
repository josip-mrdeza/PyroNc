using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using TrCore;
using TrCore.Logging;
using UnityEngine;
using Random = System.Random;

namespace Pyro.Nc.Pathing
{
    public class ToolDebug : MonoBehaviour, ITool
    {
        public ValueStorage Storage { get; set; }
        public Vector3 Position { get; set; }
        public Path CurrentPath { get; set; }
        public PObject Workpiece { get; set; }
        public TimeSpan FastMoveTick = TimeSpan.FromMilliseconds(0.1d);
        public ICommand Current;
        public Target Destination;
        public Vector3 CurrentLocation => transform.position;
        public event Func<ICommand, Vector3, Task> OnCollision;
        public async void Start()
        {
            Destination = new Target(new Vector3());
            Workpiece = new PObject();
            Storage = await ValueStorage.CreateFromFile(this);
        }
        
        public async Task UseCommand(ICommand command)
        {
            Current = command;
            await command.Execute();
            Current = null;
        }
        protected virtual async Task UntilValid()
        {
            if (Destination.IsValid)
            {
                while (Destination.IsValid)
                {
                    await Task.Yield();
                    await Task.Delay(FastMoveTick);
                }
            }
        }
        protected virtual Vector3 SetupMove(Vector3[] points, out Random rnd)
        {
            Destination = new Target(points.Last());
            CurrentPath = new Path(points);
            Vector3 prev = transform.position;
            rnd = new Random();

            return prev;
        }
        protected virtual void DrawLine(bool draw, Random rnd, Vector3 prev, Vector3 p)
        {
            if (draw)
            {
                var val = rnd.Next(10, 100) / 100f;
                var color = new Color(1, val + 0.1f, val - 0.2f, 1f);
                Debug.DrawLine(prev, p, color, 1f);
            }
        }
        protected virtual async Task CheckPositionForCut()
        {
            if (Workpiece.IsPointInside(Position.ToVector3D()))
            {
                if (OnCollision is not null)
                {
                    await OnCollision.Invoke(Current, Position);
                }
            }
        }
        public virtual async Task Traverse(Vector3[] points, bool draw)
        {
            await UntilValid();
            var prev = SetupMove(points, out var rnd);
            foreach (var point in points)
            {
                await Task.Yield();
                transform.position = point;
                DrawLine(draw, rnd, prev, point);
                prev = point;
                await CheckPositionForCut();
                await Task.Delay(FastMoveTick);
            }
        }
        public virtual async Task Traverse(Line3D line, bool draw)
        {
            await Traverse(line.ToVector3s(), draw);
        }
        public virtual async Task Traverse(Vector3 toPoint, bool draw)
        {
            await Traverse(new Line3D(CurrentLocation.ToVector3D(), toPoint.ToVector3D(), 
                                      (int) LineTranslationSmoothness.Standard), draw);
        }
        public virtual async Task Traverse(Circle3D circle, bool draw)
        {
            await Traverse(circle.ToVector3s(), draw);
        }

        public virtual async Task Traverse(Vector3 circleCenter, float circleRadius, bool draw)
        {
            await Traverse(circleCenter, circleRadius, false, draw);
        }
        
        public async Task Traverse(Vector3 circleCenter, float circleRadius, bool reverse, bool draw)
        {
            await Traverse(new Circle3D(circleRadius, Position.y).Mutate(c =>
            {
                c.SwitchYZ();
                c.Shift(circleCenter.ToVector3D());
                if (reverse)
                {
                    c.Reverse();
                }
                return c;
            }), draw);
        }
    }
}