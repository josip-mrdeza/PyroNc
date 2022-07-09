namespace Pyro.Math.Geometry
{
    public readonly struct LinePoint
    {
        public readonly Vector3D Position;
        public readonly int Index;

        public LinePoint(Vector3D position, int index)
        {
            Position = position;
            Index = index;
        }
    }
}