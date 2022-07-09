namespace Pyro.Nc
{
    public struct MArgs
    {
        public float X;
        public float Y;
        public float Z;

        public bool IsValid;

        public MArgs(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            IsValid = true;
        }
    }
}