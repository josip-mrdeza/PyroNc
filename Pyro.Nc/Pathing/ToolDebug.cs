using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using Pyro.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Pyro.Nc.Pathing
{
    public class ToolDebug : MonoBehaviour, ITool
    {
        public Mesh meshPointer;
        public GameObject Plane;
        public GameObject PObj;
        public Triangulator Triangulator { get; set; }
        public Vector3 Position => transform.position;
        public PObject Workpiece { get; set; }
        public ToolValues Values { get; set; }
        public event Func<Task> OnConsumeStopCheck;
        public event Func<ICommand, Vector3, Task> OnCollision;
        public ComputeBuffer Buffer;

        public async Task InvokeOnConsumeStopCheck()
        {
            if (OnConsumeStopCheck != null)
            {
                await OnConsumeStopCheck();
            }
        }

        private void Awake()
        {
            OnCollision += Cut;
            Values = new ToolValues(this);
            Globals.Tool = this;
        }

        public void Start()
        {
            meshPointer ??= Plane.GetComponent<MeshFilter>().mesh;
            Workpiece = new PObject(meshPointer);
            Triangulator = new Triangulator(Workpiece.Mesh);
            Plane.GetComponent<MeshFilter>().mesh = Triangulator.CurrentMesh;
            Collider = Plane.GetComponent<MeshCollider>();
            var bounds = Collider.bounds;
            var tr = Plane.transform;
            var max = tr.TransformVector(bounds.max);
            maxX = max.x;
            maxY = max.y;
            maxZ = max.z;
            var min = tr.TransformVector(bounds.min);
            minX = min.x;
            minY = min.y;
            minZ = min.z;
        }

        public async Task UseCommand(ICommand command, bool draw)
        {
            await command.ExecuteFinal(draw);
        }
        public async Task UseCommandDebugDraw(ICommand command)
        {
            await UseCommand(command, true);
        }
        public virtual async Task UntilValid()
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

        public float minX = 0f;
        public float minY = 0f;
        public float minZ = 0f;
        public MeshCollider Collider;
        public float maxX;
        public float maxY;
        public float maxZ;
        /*public virtual Direction IsOverLimit(Vector3 dest)
        {
            var d = new Direction();
            if (dest.x < minX || dest.x > maxX)
            {
                d.X = dest.x.Abs();
            }
            if (dest.y < minY || dest.y > maxY)
            {
                d.Y = dest.y.Abs();
            }
            if (dest.z < minZ || dest.z > maxZ)
            {
                d.Z = dest.z.Abs();
            }

            return d;
        }*/
        
        public virtual bool IsOkayToCut(Vector3 dest)
        {
            var d = new Direction();
            if (dest.x < minX || dest.x > maxX)
            {
                d.X = dest.x.Abs();
            }
            if (dest.y < minY || dest.y > maxY)
            {
                d.Y = dest.y.Abs();
            }
            if (dest.z < minZ || dest.z > maxZ)
            {
                d.Z = dest.z.Abs();
            }

            return d.X == 0 && d.Y == 0 && d.Z == 0;
        }
        protected virtual Task CheckPositionForCut(Direction direction)
        {
            //find the fastest way to traverse an array and find the appropriate vertex to 'remove'.
            Stopwatch stopwatch = Stopwatch.StartNew();
            var pos = Position;
            var verts = Triangulator.CurrentMesh.vertices;
            var tr = Plane.transform;
            var v = new Vector3(direction.X, direction.Y, direction.Z);
            for (int i = 0; i < verts.Length; i++)
            {
                ref var vert = ref verts[i];
                var realVert = tr.TransformVector(vert);
                var distance = Vector3.Distance(pos, realVert);
                var result = vert - v;
                //where we remove a vertex, we append a new one
                if (distance < 0.3F && IsOkayToCut(result))
                {
                    var final = v;
                    vert -= final;
                }
            }

            Triangulator.CurrentMesh.vertices = verts;
            stopwatch.Stop();  
            return Task.CompletedTask;
        }
        public virtual async Task Traverse(Vector3[] points, bool draw)
        {
            await UntilValid();
            var prev = SetupMove(points, out var rnd);
            foreach (var point in points)
            {
                var pos = point;
                transform.position = pos;
                DrawLine(draw, rnd, prev, pos);
                await CheckPositionForCut(Direction.FromVectors(prev, pos));
                await Task.Delay(Values.FastMoveTick);
                await Task.Yield();
                prev = pos;
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
        public virtual async Task Traverse(Vector3 toPoint, LineTranslationSmoothness smoothness, bool draw = false)
        {
            await Traverse(new Line3D(Position.ToVector3D(), toPoint.ToVector3D(), (int) smoothness), 
                           draw);
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
                if (reverse)
                {
                    c.Reverse();
                }
                c.Shift(circleCenter.ToVector3D());
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
