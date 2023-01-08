using System;
using System.Linq;

namespace Pyro.Math.Geometry
{
    public class Circle : IPrimitive
    {
        public float Radius { get; }
        public Vector2D[] Points { get; }
        public CircleSmoothness Smoothness { get; }
        public Shape GeometricalShape { get; }

        public Circle(float radius, CircleSmoothness smoothness = CircleSmoothness.Standard)
        {
            Radius = radius;
            Points = Space2D.PlotCircle2D(radius, smoothness);
            Smoothness = smoothness;
            GeometricalShape = Shape.Circle;
        }
        
        /// <summary>
        /// Recalculated the coordinates with the shifted values.
        /// </summary>
        public void ShiftCenter(float x, float y)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                p.x += x;
                p.y += y;
            }
        }
    }
}