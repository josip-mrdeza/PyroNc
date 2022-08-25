using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using UnityEngine;
using Random = System.Random;

namespace Pyro.Nc.Pathing
{
    public class ToolDebug : MonoBehaviour, ITool
    {
        public Mesh meshPointer;
        public GameObject Plane;
        public ValueStorage Storage { get; set; }
        public Triangulator Triangulator { get; set; }

        public Vector3 Position
        {
            get => transform.position;
        }
        public Path CurrentPath { get; set; }
        public PObject Workpiece { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsIncremental { get; set; }
        public bool IsImperial { get; set; }
        public ICommand Current { get; set; }
        public bool ExactStopCheck { get; set; }
        public event Func<Task> OnConsumeStopCheck;

        public async Task InvokeOnConsumeStopCheck()
        {
            if (OnConsumeStopCheck != null)
            {
                await OnConsumeStopCheck();
            }
        }
        public TimeSpan FastMoveTick = TimeSpan.FromMilliseconds(0.1d);
        public Target Destination;
        public event Func<ICommand, Vector3, Task> OnCollision;
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
        public async void Start()
        {
            meshPointer ??= Plane.GetComponent<MeshFilter>().mesh;
            Workpiece = new PObject(meshPointer);
            Triangulator = new Triangulator(Workpiece.Mesh);
            IsAllowed = true;
            Destination = new Target(new Vector3());
            Storage = await ValueStorage.CreateFromFile(this);
            OnCollision += Cut;
        }
        
        public async Task UseCommand(ICommand command, bool draw)
        {
            await command.ExecuteFinal(draw);
        }
        public async Task UseCommandDebugDraw(ICommand command)
        {
            await command.Execute(true);
        }
        protected virtual async Task UntilValid()
        {
            if (Destination.IsValid)
            {
                while (Destination.IsValid && !IsAllowed)
                {
                    await Task.Yield();
                    await Task.Delay(FastMoveTick, Current.Parameters.Token);
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
            else
            {
                Debug.Log($"[{(Current is null ? "nullC" : Current.Description)}] Tool did not cut the workpiece at position {Position.ToString()}");
            }
        }
        public virtual async Task Traverse(Vector3[] points, bool draw)
        {
            await UntilValid();
            var prev = SetupMove(points, out var rnd);
            foreach (var point in points)
            {
                await Task.Yield();
                var pos = point;
                if (IsIncremental)
                {
                    pos += transform.position;
                }
                transform.position = pos;
                DrawLine(draw, rnd, prev, pos);
                prev = pos;
                await CheckPositionForCut();
                await Task.Delay(FastMoveTick);
                if (Current.Parameters.Token.IsCancellationRequested)
                {
                    return;
                }
            }

            if (ExactStopCheck)
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