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
            LinePoint? last = Points[0] = new LinePoint(Start, 0);
            var endPoint = numOfPoints - 1;
            Points[endPoint] = new LinePoint(End, endPoint);
            // for (int i = 1; i < endPoint; i++)
            // {
            //     var lp0 = last.Value;
            //     var lp1 = new LinePoint(Space3D.MiddlePoint(p2, lp0.Position), lp0.Index + 1);
            //     last = lp1;
            //     Points[i] = lp1;
            // }
            
            //The line is split into 'endPoint' many equal parts.
            var max = Space3D.DistanceByAxis(p1, p2);
            
            var maxX = max.x;
            var maxY = max.y;
            var maxZ = max.z;

            var eqX = maxX / numOfPoints;
            var eqY = maxY / numOfPoints;
            var eqZ = maxZ / numOfPoints;
            
            for (int i = 1; i < endPoint; i++)
            {
                Points[i] = new LinePoint(new Vector3D(eqX * i, eqY * i, eqZ * i), i);
            }
        }
    }
}