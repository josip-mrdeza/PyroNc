namespace Pyro.Math.Geometry
{
    public readonly struct Arc3D
    {
        private readonly Circle3D _circle3D;
        public Vector3D[] Points { get; }
        public Vector3D Start { get; }
        public Vector3D End { get; }
        
    }
}