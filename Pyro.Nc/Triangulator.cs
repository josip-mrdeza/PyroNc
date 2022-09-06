using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc
{
    public class Triangulator
    {
        public Mesh OriginalMesh;
        public Mesh CurrentMesh;
        public Triangulator(Mesh mesh)
        {
            OriginalMesh = mesh;
            CurrentMesh = new Mesh();
            CurrentMesh.vertices = mesh.vertices;
            CurrentMesh.uv = mesh.uv;
            CurrentMesh.triangles = mesh.triangles;
        }

        public void RemoveVertex(int num)
        {
            var length = CurrentMesh.vertices.Length;
            var verts = new Vector3?[length];
            for (int i = 0; i < length; i++)
            {
                if (i != num)
                {
                    verts[i] = CurrentMesh.vertices[i];
                }
            }

            var vertices = verts.Where(v => v != null).Select(vt => vt.Value);
            
            List<int> trigs = new List<int>(CurrentMesh.triangles.Length);
            for (int i = 0; i < CurrentMesh.triangles.Length; i+=3)
            {
                var trigV0 = CurrentMesh.triangles[i];
                var trigV1 = CurrentMesh.triangles[i + 1];
                var trigV2 = CurrentMesh.triangles[i + 2];
                if (trigV0 != num && trigV1 != num && trigV2 != num)
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }
            }

            var trigsArr1 = trigs.Select(t => t > num ? t - 1 : t).ToArray();
            var trigsArr = trigsArr1.Concat(trigsArr1.Reverse());

            CurrentMesh.Clear();
            CurrentMesh.vertices = vertices.ToArray();
            CurrentMesh.triangles = trigsArr.ToArray();  
        }

        public (Vector3[], int[]) RemoveVertices(int[] indices)
        {
            List<Vector3> verts = CurrentMesh.vertices.ToList();
            foreach (var index in indices)
            {
                verts.RemoveAt(index);
            }
            
            List<int> trigs = new List<int>(CurrentMesh.triangles.Length);
            var ogTrigs = CurrentMesh.triangles;
            for (int i = 0; i < ogTrigs.Length; i++)
            {
                var trigV0 = ogTrigs[i];
                var trigV1 = ogTrigs[i + 1];
                var trigV2 = ogTrigs[i + 2];

                if (indices.All(c => trigV0 != c && trigV1 != c && trigV2 != c))
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }
            }

            var min = indices.Min();
            var trigsArr1 = trigs.Select(t => t > min ? t - indices.Length : t).ToArray();
            
            var vertices = verts.ToArray();
            var trigsArr = trigsArr1.Concat(trigsArr1.ToArray().Reverse()).ToArray();
            CurrentMesh.Clear();
            CurrentMesh.vertices = vertices;
            CurrentMesh.triangles = trigsArr;

            return (vertices, trigsArr);
        }
        
        public void RemoveVerticesFake(int[] indices)
        {
            List<int> trigs = new List<int>(CurrentMesh.triangles.Length);
            var ogTrigs = CurrentMesh.triangles;
            for (int i = 0; i < ogTrigs.Length; i+=3)
            {
                var trigV0 = ogTrigs[i];
                var trigV1 = ogTrigs[i + 1];
                var trigV2 = ogTrigs[i + 2];

                if (indices.All(c => trigV0 != c && trigV1 != c && trigV2 != c))
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }
            }

            var trigsArr = trigs.Concat(trigs.AsEnumerable().Reverse()).ToArray();
            CurrentMesh.triangles = trigsArr;
        }
        
        public async Task RemoveVerticesFakeAsync(int[] indices)
        {
            List<int> trigs = new List<int>(CurrentMesh.triangles.Length);
            // var ogTrigs = new int[CurrentMesh.triangles.Length];
            // CurrentMesh.triangles.CopyTo(ogTrigs, 0);
            var ogTrigs = CurrentMesh.triangles;
            for (int i = 0; i < ogTrigs.Length; i+=3)
            {
                var trigV0 = ogTrigs[i];
                var trigV1 = ogTrigs[i + 1];
                var trigV2 = ogTrigs[i + 2];

                if (indices.All(c => trigV0 != c && trigV1 != c && trigV2 != c))
                {
                    trigs.Add(trigV0);
                    trigs.Add(trigV1);
                    trigs.Add(trigV2);
                }

                await Task.Yield();
            }

            var trigsArr = trigs.Concat(trigs.AsEnumerable().Reverse()).ToArray();
            CurrentMesh.triangles = trigsArr;
        }
    }
}