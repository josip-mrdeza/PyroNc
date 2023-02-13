namespace Pyro.Math
{
    public partial struct Vector3D
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

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
        
        public static Vector3D operator+(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public override string ToString() => $"({x}, {y}, {z})";
    }
}