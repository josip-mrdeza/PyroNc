namespace Pyro.Math
{
    public partial struct Vector3D
    {
        public float x;
        public float y;
        public float z;

        public Vector3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static explicit operator Vector2D(Vector3D v3d)
        {
            return new Vector2D(v3d.x, v3d.y);
        }

        public static implicit operator Vector3D((float x, float y, float z) tuple)
        {
            return new Vector3D(tuple.x, tuple.y, tuple.z);
        }
    }
}