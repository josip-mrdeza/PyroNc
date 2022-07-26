
namespace Pyro.Math.Geometry
{
    public readonly struct Line3D
    {
        public readonly Vector3D Start;
        public readonly Vector3D End;
        public readonly LinePoint[] Points;
        public readonly int NumberOfPoints;
        
        public Line3D(Vector3D p1, Vector3D p2, int numOfPoints = 10)
        {
            Start = p1;
            End = p2;
            Points = new LinePoint[numOfPoints];
            NumberOfPoints = numOfPoints;
            int endPoint = numOfPoints - 1;
            Points[endPoint] = new LinePoint(End, endPoint);
            var max = Space3D.DistanceByAxis(p1, p2);
            var maxX = max.x;
            var maxY = max.y;
            var maxZ = max.z;
            var eqX = maxX / numOfPoints;
            var eqY = maxY / numOfPoints;
            var eqZ = maxZ / numOfPoints;
            
            for (int i = 1; i < endPoint; i++)
            {
                Points[i] = new LinePoint(new Vector3D((eqX * i), (eqY * i), (eqZ * i)), i);
            }

            for (int i = 1; i < endPoint; i++)
            {
                var pt = Points[i];
                var p = pt.Position;
                p.x += p1.x;
                p.y += p1.y;
                p.z += p1.z;
                Points[i] = new LinePoint(p, pt.Index);
            }
        }
    }
}