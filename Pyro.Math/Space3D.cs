using System;
using System.Linq;
using Pyro.Math.Geometry;

namespace Pyro.Math
{
    public static partial class Space3D
    {
        /// <summary>
        /// Calculates the distance between 2 points.
        /// </summary>
        /// <returns>The distance from 'point1' to 'point2' in 3D space.</returns>
        public static float Distance(Vector3D point1, Vector3D point2)
        {
            return ((point2.x - point1.x).Squared() + (point2.y - point1.y).Squared() + (point2.z - point1.z).Squared())
                .SquareRoot();
        }

        public static Vector3D DistanceByAxis(Vector3D point1, Vector3D point2)
        {
            return new Vector3D(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        }

        public static Vector3D MiddlePoint(Vector3D v1, Vector3D v2)
        {
            var middleX = Operations.Average(v1.x, v2.x);
            var middleY = Operations.Average(v1.y, v2.y);
            var middleZ = Operations.Average(v1.z, v2.z);

            return new Vector3D(middleX, middleY, middleZ);
        }

        /// <summary>
        /// Plots the circle on the base coordinates of the cartesian's coordinate system, which is the origin (x,y,z) = (0,0,0) with it's radius of r.
        /// </summary>
        /// <param name="circleSmoothness">Represents the amount of points the circle is to be plotted with.</param>
        /// <param name="r">The radius of the circle to be plotted.</param>
        /// <param name="depth">The depth (Y) at which the circle is to be plotted on.</param>
        /// <returns>A collection of points representing a circle in 3D space with an infinitely small volume.</returns>
        public static Vector3D[] PlotCircle3D(float r, float depth, CircleSmoothness circleSmoothness = CircleSmoothness.Fine)
        {
            Vector3D[] arr = new Vector3D[(int) circleSmoothness];
            var multiplier = 360f / arr.Length;
            if (multiplier >= 1) //8
            {
                for (int i = 0; i < 360;  i += (int) multiplier)
                {
                    arr[(int) (i / multiplier)] = new Vector3D(-(i.Cos() * r), i.Sin() * r, -depth);
                }
            }
            else
            {
                for (int i = 0; i < 360; i++)
                {
                    arr[i] = new Vector3D(-((i * multiplier).Cos() * r), (i * multiplier).Sin() * r, -depth);
                }
            }

            return arr;
        }

        public static Vector3D[] PlotCircle3D(float r, Vector3D circleCenterPoint, CircleSmoothness circleSmoothness = CircleSmoothness.Fine)
        {
            Vector3D[] arr = PlotCircle3D(r, circleCenterPoint.z, circleSmoothness);
            for (int i = 0; i < arr.Length; i++)
            {
                var a = arr[i];
                a.x = circleCenterPoint.x + a.x;
                a.y = circleCenterPoint.y + a.y;
                arr[i] = a;
            }
            return arr;
        }

        public static bool IsInside(this Vector3D pointInSpace, I3DShape shape)
        {
            return shape.XLimits.All(x => x.IsValidFor(pointInSpace.x))
                && shape.YLimits.All(y => y.IsValidFor(pointInSpace.y))
                && shape.ZLimits.All(z => z.IsValidFor(pointInSpace.z));
        }

        public static bool IsTouching(this Vector3D pointInSpace, I3DShape shape)
        {
            return (shape.XLimits.All(x => x.IsAtBorder(pointInSpace.x)) 
                &&
                shape.YLimits.All(y => y.IsAtBorder(pointInSpace.y))
                &&
                !shape.ZLimits.All(z => z.IsAtBorder(pointInSpace.z)))
                || 
                (shape.ZLimits.All(z => z.IsAtBorder(pointInSpace.z))
                &&
                shape.YLimits.All(y => y.IsAtBorder(pointInSpace.y))
                && 
                !shape.XLimits.All(x => x.IsAtBorder(pointInSpace.x)));
        }
        
    }
}