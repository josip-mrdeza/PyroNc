using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using UnityEngine;
using Random = System.Random;

namespace Pyro.Nc.Pathing
{
    public class ToolDebug : MonoBehaviour, ITool
    {
        public Mesh meshPointer;
        public GameObject Plane;
        public Triangulator Triangulator { get; set; }
        public Vector3 Position => transform.position;
        public PObject Workpiece { get; set; }
        public ToolValues Values { get; set; }
        public event Func<Task> OnConsumeStopCheck;
        public event Func<ICommand, Vector3, Task> OnCollision;

        public async Task InvokeOnConsumeStopCheck()
        {
            if (OnConsumeStopCheck != null)
            {
                await OnConsumeStopCheck();
            }
        }
        public async void Start()
        {
            meshPointer ??= Plane.GetComponent<MeshFilter>().mesh;
            Workpiece = new PObject(meshPointer);
            Triangulator = new Triangulator(Workpiece.Mesh);
            Values = new ToolValues(this);
            OnCollision += Cut;
        }
        
        public async Task UseCommand(ICommand command, bool draw)
        {
            await command.ExecuteFinal(draw);
        }
        public async Task UseCommandDebugDraw(ICommand command)
        {
            await command.ExecuteFinal(true);
        }
        protected virtual async Task UntilValid()
        {
            if (Values.Destination.IsValid)
            {
                while (Values.Destination.IsValid && !Values.IsAllowed)
                {
                    await Task.Delay(Values.FastMoveTick, Values.Current.Parameters.Token);
                    await Task.Yield();
                }
            }
        }
        protected virtual Vector3 SetupMove(Vector3[] points, out Random rnd)
        {
            Values.Destination = new Target(points.Last());
            Values.CurrentPath = new Path(points);
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
                    await OnCollision.Invoke(Values.Current, Position);
                }
            }
            else
            {
                Debug.Log($"[{(Values.Current is null ? "nullC" : Values.Current.Description)}] Tool did not cut the workpiece at position {Position.ToString()}");
            }
        }
        public virtual async Task Traverse(Vector3[] points, bool draw)
        {
            await UntilValid();
            var prev = SetupMove(points, out var rnd);
            foreach (var point in points)
            {
                var pos = point;
                if (Values.IsIncremental)
                {
                    pos += transform.position;
                }
                transform.position = pos;
                DrawLine(draw, rnd, prev, pos);
                prev = pos;
                await CheckPositionForCut();
                await Task.Delay(Values.FastMoveTick);
                await Task.Yield();
                if (Values.Current.Parameters.Token.IsCancellationRequested)
                {
                    return;
                }
            }

            if (Values.ExactStopCheck)
            {
                //Already is at the exact pos, so no need to do checks.
                if (OnConsumeStopCheck != null)
                {
                    await OnConsumeStopCheck();
                }
            }
        }
        public virtual async Task Traverse(Line3D line, bool draw)
        {
            await Traverse(line.ToVector3s(), draw);
        }
        public virtual async Task Traverse(Vector3 toPoint, LineTranslationSmoothness smoothness = LineTranslationSmoothness.Crude, bool draw = false)
        {
            await Traverse(new Line3D(Position.ToVector3D(), toPoint.ToVector3D(), 
                                      (int) smoothness), draw);
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
                if (System.Math.Abs(Position.y - circleCenter.y) < 0.1f)
                {
                    circleCenter.y = 0;
                }
                c.Shift(circleCenter.ToVector3D());

                if (reverse)
                {
                    c.Reverse();
                }
                Debug.Log($"Sample 0: {c.Points[0].ToVector3().ToString()}");
                return c;
            }), draw);
        }

        protected virtual async Task Cut(ICommand command, Vector3 position)
        {
            // Debug.Log($"Before cut: Triangles: {meshPointer.triangles.Length}; Verts: {meshPointer.vertices.Length}!");
            // await Triangulator.TriangulateMesh();
            // Debug.Log($"After cut: Triangles: {meshPointer.triangles.Length}; Verts: {meshPointer.vertices.Length}!");
        }
        
    }
}
