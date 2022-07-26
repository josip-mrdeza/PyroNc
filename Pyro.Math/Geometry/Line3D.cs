
using System;
using System.Linq;

namespace Pyro.Math.Geometry
{
    public class Line3D : I3DShape, IPrimitive
    {
        public readonly Vector3D Start;
        public readonly Vector3D End;
        public readonly LinePoint[] LinePoints;
        public readonly int NumberOfPoints;
        public readonly string ExplicitEquation;
        public readonly string ImplictEquation;
        public Vector3D[] Points { get; }
        public Limit[] XLimits { get; }
        public Limit[] YLimits { get; }
        public Limit[] ZLimits { get; }

        public Vector3D this[int num]
        {
            get => Points[num];
            set => Points[num] = value;
        } 
        public Shape GeometricalShape { get; }
        public Line3D(Vector3D p1, Vector3D p2, int numOfPoints = 10)
        {
            Start = p1;
            End = p2;
            LinePoints = new LinePoint[numOfPoints > 1 ? numOfPoints : throw new ArgumentException("Argument 'numOfPoints' cannot deviate from a set <1, Int32.MaxValue>!")];
            NumberOfPoints = numOfPoints;
            int endPoint = numOfPoints - 1;
            
            LinePoints[0] = new LinePoint(p1, 0);
            LinePoints[endPoint] = new LinePoint(End, endPoint);
            
            var max = Space3D.DistanceByAxis(p1, p2);
            var maxX = max.x;
            var maxY = max.y;
            var maxZ = max.z;
            var eqX = maxX / numOfPoints;
            var eqY = maxY / numOfPoints;
            var eqZ = maxZ / numOfPoints;
            
            for (int i = 1; i < endPoint; i++)
            {
                LinePoints[i] = new LinePoint(new Vector3D((eqX * i), (eqY * i), (eqZ * i)), i);
            }

            for (int i = 1; i < endPoint; i++)
            {
                var pt = LinePoints[i];
                var p = pt.Position;
                p.x += p1.x;
                p.y += p1.y;
                p.z += p1.z;
                LinePoints[i] = new LinePoint(p, pt.Index);
            }

            var x1 = p1.x;
            var y1 = p1.y;

            var x2 = p2.x;
            var y2 = p2.y;

            var f1 = (y2 - y1);
            var f2 = (x2 - x1);

            var k = f1 / f2;

            char sign = k > 0 ? '+' : '-';
            
            ExplicitEquation = $"y = {k}x {sign}{System.Math.Abs(k * x1)}";
            ImplictEquation = null;
            GeometricalShape = Shape.Line;
            Points = LinePoints.Select(x => x.Position).ToArray();
            XLimits = new Limit[]
            {
            };
            YLimits = new Limit[]
            {
            };
            ZLimits = new Limit[]
            {
            };
        }
    }
}