namespace Pyro.Math
{
    public static partial class Space2D
    {
        public const float DegreeToRadian = (float) System.Math.PI / 180f;
        /// <summary>
        /// Calculates the distance between 2 points.
        /// </summary>
        /// <returns>The distance from 'point1' to 'point2' in 2D space.</returns>
        public static float Distance(Vector2D point1, Vector2D point2)
        {
            return ((point2.x - point1.x).Squared() + (point2.y - point1.y).Squared()).SquareRoot(); 
        }

        public static Vector2D DistanceByAxis(Vector2D point1, Vector2D point2)
        {
            return new Vector2D(point2.x - point1.x, point2.y - point1.y);
        }

        /// <summary>
        /// Plots the circle on the base coordinates of the cartesian's coordinate system, which is the origin (x,y) = (0,0) with it's radius of r.
        /// </summary>
        /// <param name="circleSmoothness">Represents the amount of points the circle is to be plotted with.</param>
        /// <param name="r">The radius of the circle to be plotted.</param>
        /// <returns>A collection of points representing a circle in 3D space with an infinitely small volume.</returns>
        public static Vector2D[] PlotCircle2D(float r, CircleSmoothness circleSmoothness = CircleSmoothness.Fine)
        {
            Vector2D[] arr = new Vector2D[(int) circleSmoothness];
            var multiplier = 360f / arr.Length;
            if (multiplier >= 1) //8
            {
                for (int i = 0; i < 360;  i += (int) multiplier)
                {
                    arr[(int) (i / multiplier)] = new Vector2D(i.Cos() * r, i.Sin() * r);
                }
            }
            else
            {
                for (int i = 0; i < 360; i++)
                {
                    arr[i] = new Vector2D((i * multiplier).Cos() * r, (i * multiplier).Sin() * r);
                }
            }

            return arr;
        }
        
        public static Vector2D[] PlotCircle2D(float radius, Vector2D centerPoint)
        {
            Vector2D[] arr = PlotCircle2D(radius);
            for (int i = 0; i < arr.Length; i++)
            {
                ref var a = ref arr[i];
                a.x = centerPoint.x - a.x;
                a.y = centerPoint.y - a.y;
            }

            return arr;
        }
    }
}