namespace Pyro.Nc
{
    public struct GArgs
    {
        public float X;
        public float Y;
        public float Z;

        public bool IsValid;

        public GArgs(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            IsValid = true;
        }

        public override string ToString()
        {
            return $"Coord: {X}, {Y}, {Z}";
        }
    }
}
