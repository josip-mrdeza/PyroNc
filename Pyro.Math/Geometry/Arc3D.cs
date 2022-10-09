using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pyro.IO;

namespace Pyro.Math.Geometry
{
    public class Arc3D : I3DShape
    {
        private Circle3D _circle3D;
        public Vector3D[] Points { get; set; }
        public Limit[] XLimits { get; }
        public Limit[] YLimits { get; }
        public Limit[] ZLimits { get; }

        public Vector3D this[int num]
        {
            get => Points[num];
            set => Points[num] = value;
        }
        public float Radius { get; }      
        public float Depth { get; }
        public Vector3D Start { get; }
        public Vector3D End { get; }
        
        public readonly CircleSmoothness Smoothness;      
        public Shape GeometricalShape { get; } 
        /// <summary>
        /// Used with force based movement.
        /// </summary>
        public Arc3D(float radius, Vector3D start, int degrees, float depth, CircleSmoothness smoothness = CircleSmoothness.Rough)
        {
            Radius = radius;
            Depth = depth;
            Smoothness = smoothness;
            GeometricalShape = Shape.Circle;
            Start = start;
            End = Points.Last();
            Points = PlotArc(degrees);
        }
        
        public Arc3D(float radius, Vector3D start, Vector3D endPoint, Vector3D center, Vector3D rotateTowards, float depth, CircleSmoothness smoothness = CircleSmoothness.Rough)
        {
            Radius = radius;
            Depth = depth;
            Smoothness = smoothness;
            GeometricalShape = Shape.Circle;
            Start = start;
            End = endPoint;
            Points = PlotArc(center, rotateTowards);
        }

        public Vector3D[] PlotArc(int degrees)
        {
            var arr = new Vector3D[(int) Smoothness];
            var multiplier = (float) degrees / arr.Length;
            if (multiplier >= 1) //8
            {
                for (int i = 0; i < degrees;  i += (int) multiplier)
                {
                    arr[(int) (i / multiplier)] = new Vector3D(-(i.Cos() * Radius), Depth, (i.Sin() * Radius));
                }
            }
            else
            {
                for (int i = 0; i < degrees; i++)
                {
                    arr[i] = new Vector3D(-((i * multiplier).Cos() * Radius), Depth, (i * multiplier).Sin() * Radius);
                }
            }
            return arr;
        }
        
        public Vector3D[] PlotArc(Vector3D shift, Vector3D rotateTowards)
        {
            var arr = new List<Vector3D>((int) Smoothness);
            var p1 = new Vector3D(-Radius + shift.x, Depth, shift.z);
            
            var rotateDegrees = (float) (System.Math.Atan2(rotateTowards.z - p1.z, rotateTowards.x - p1.x) * (180f/System.Math.PI)) * 2;
            for (int i = 0; i < 360;  i++)
            {
                var v3 = new Vector3D(-((i + rotateDegrees).Cos() * Radius) + shift.x, Depth, ((i + rotateDegrees).Sin() * Radius) + shift.z);
                if (Space3D.Distance(v3, End) < 0.00001f)
                {
                    //return arr.ToArray();
                }
                arr.Add(v3);
            }
            return arr.ToArray();
        }
        
        /// <summary>
        /// Recalculated the coordinates with the shifted values.
        /// </summary>
        public void Shift(float x, float y, float z)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                p.x += x;
                p.z += z;
                Points[i] = p;
            }
        }

        public void Shift(Vector3D vector3D)
        {
            Shift(vector3D.x, vector3D.y, vector3D.z);
        }

        internal static readonly Vector3D[] _RevArr = new Vector3D[360];
    }
}