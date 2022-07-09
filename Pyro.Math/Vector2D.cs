namespace Pyro.Math
{
    public partial struct Vector2D
    {
        public float x;
        public float y;

        public Vector2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static explicit operator Vector3D(Vector2D v2d)
        {
            return new Vector3D(v2d.x, v2d.y, 0);
        }
    }
}