using System.Linq;

namespace Pyro.Math.Geometry
{
    public class Circle3D : I3DShape, IPrimitive
    {
        public readonly float Radius;
        public readonly Vector3D Start;
        public readonly Vector3D End;

        public Vector3D[] Points { get; }
        public Limit[] XLimits { get; }
        public Limit[] YLimits { get; }
        public Limit[] ZLimits { get; }

        public Vector3D this[int num]
        {
            get => Points[num];
            set => Points[num] = value;
        }

        public readonly CircleSmoothness Smoothness;      
        public Shape GeometricalShape { get; }

        public Circle3D(float radius, float depth, CircleSmoothness smoothness = CircleSmoothness.Standard)
        {
            Radius = radius;
            Points = Space3D.PlotCircle3D(radius, depth, smoothness);
            Start = Points[0];
            End = Start;
            Smoothness = smoothness;
            XLimits = new[]
            {
                new Limit(Axis3D.X, -radius, radius)
            };
            YLimits = new[]
            {
                new Limit(Axis3D.Y, -radius, radius)
            };
            ZLimits = new[]
            {
                new Limit(Axis3D.Z, -depth / 2, depth / 2)
            };
            GeometricalShape = Shape.Circle;
        }

        public Circle3D(float radius, Vector3D startPoint, Vector3D endPoint, CircleSmoothness smoothness = CircleSmoothness.Standard)
        {
            Radius = radius;
            var depth = Operations.Average(startPoint.z, endPoint.z);
            Points = Space3D.PlotCircle3D(radius, depth, smoothness);
            Start = startPoint;
            End = endPoint;
            Smoothness = smoothness;
            XLimits = new[]
            {
                new Limit(Axis3D.X, -radius, radius)
            };
            YLimits = new[]
            {
                new Limit(Axis3D.Y, -radius, radius)
            };
            ZLimits = new[]
            {
                new Limit(Axis3D.Z, -depth / 2, depth / 2)
            };
            GeometricalShape = Shape.Circle;
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

        public void SwitchYZ()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                ref var p = ref Points[i];
                p = new Vector3D(p.x, p.z, p.y);
            }
        }

        public void Reverse()
        {
            for (int i = Points.Length - 1; i >= 0; i--)
            {
                _RevArr[i] = Points[Points.Length - 1 - i];
            }

            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = _RevArr[i];
                _RevArr[i] = default;
            }
        }

        private static Vector3D[] _RevArr = new Vector3D[360];
    }
}