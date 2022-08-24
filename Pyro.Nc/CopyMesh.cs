using System;
using UnityEngine;

namespace Pyro.Nc
{
    public class CopyMesh : MonoBehaviour
    {
        public Mesh Altered;

        private void Awake()
        {
            var comp = GetComponent<MeshFilter>();
            var og = comp.mesh;
            Altered = new Mesh();
            Altered.vertices = og.vertices;
            Altered.triangles = og.triangles;
            Altered.normals = og.normals;
            Altered.uv = og.uv;
            comp.mesh = Altered;
        }
    }
}