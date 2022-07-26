namespace Pyro.Math.Geometry
{
    public class ComplexShape : I3DShape
    {
        public Vector3D[] Points { get; }
        public Limit[] XLimits { get; }
        public Limit[] YLimits { get; }
        public Limit[] ZLimits { get; }

        public Vector3D this[int num]
        {
            get => Points[num];
            set => Points[num] = value;
        }

        public ComplexShape(Vector3D[] points, Limit[] xLimits, Limit[] yLimits, Limit[] zLimits)
        {
            Points = points;
            XLimits = xLimits;
            YLimits = yLimits;
            ZLimits = zLimits;
        }
        
        

        public bool IsPointInside(Vector3D point)
        {
            return point.IsInside(this);
        }
    }
}