namespace Pyro.Math.Geometry
{
    public readonly struct Circle3D : I3DShape, IPrimitive
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
            End = Points[Points.Length - 1];
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
        public void ShiftCenter(float x, float y, float z)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                p.x += x;
                p.y += y;
                p.z += z;
                Points[i] = p;
            }
        } 
    }
}