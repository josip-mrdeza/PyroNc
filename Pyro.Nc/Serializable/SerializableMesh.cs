using System;
using UnityEngine;

namespace Pyro.Nc.Serializable
{
    [Serializable]
    public class SerializableMesh
    {
        public int[] Triangles;
        public Vector3[] Vertices;
        public Vector2[] UVs;

        public SerializableMesh(Mesh mesh)
        {
            Triangles = mesh.triangles;
            Vertices = mesh.vertices;
            UVs = mesh.uv;
        }

        public SerializableMesh()
        {
            
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(Vertices);
            mesh.SetTriangles(Triangles, 0);
            mesh.SetUVs(0, UVs);
            return mesh;
        }
    }
}