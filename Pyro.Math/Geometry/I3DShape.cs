namespace Pyro.Math.Geometry
{
    public interface I3DShape
    {
        public Vector3D[] Points { get; set; }
        public float Radius { get; }
        
        public Vector3D this[int num]
        {
            get;
            set;
        }
    }
}