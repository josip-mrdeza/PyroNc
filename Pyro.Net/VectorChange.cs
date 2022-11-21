using System.Numerics;

namespace Pyro.Net
{
    public struct VectorChange
    {
        public Vector3 Vertex;
        public int Index;

        public VectorChange(Vector3 vertex, int index)
        {
            Vertex = vertex;
            Index = index;
        }
    }
}