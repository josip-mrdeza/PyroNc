namespace Pyro.Math.Geometry
{
    public struct Limit
    {
        public Axis3D Axis;
        /// <summary>
        /// Distance from center point to the left.
        /// </summary>
        public float Left;
        /// <summary>
        /// Distance from center point to the right.
        /// </summary>
        public float Right;

        public Limit(Axis3D axis, float left, float right)
        {
            Axis = axis;
            Left = left;
            Right = right;
        }
        public bool IsValidFor(float value)
        {
            return value > Left && value < Right;   
        }

        public bool IsAtBorder(float value)
        {
            return System.Math.Abs(value - Left) < 0.01f || System.Math.Abs(value - Right) < 0.01f;
        }
    }
}