using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;
using Physics = Pyro.Math.Physics;

namespace Pyro.Nc
{
    public class Triangulator
    {
        public Mesh OriginalMesh;
        public Mesh CurrentMesh;
        public Dictionary<Vector3, int> Points = new Dictionary<Vector3, int>();
        public Triangulator(Mesh mesh)
        {
            OriginalMesh = mesh;
            CurrentMesh = new Mesh();
            CurrentMesh.vertices = mesh.vertices;
            CurrentMesh.uv = mesh.uv;
            CurrentMesh.triangles = mesh.triangles;
            // for (int i = 0; i < CurrentMesh.vertexCount; i++)
            // {
            //     var vert = CurrentMesh.vertices[i];
            //     if (!Points.ContainsKey(vert))
            //     {
            //         Points.Add(vert, i);
            //     }
            // }
        }

        public void MakeMeshDoubleSided()
        {
            var trigs = CurrentMesh.triangles;
            var altTrigs = trigs.Concat(trigs.Reverse());
            CurrentMesh.triangles = altTrigs.ToArray();
        }
        public void Remove(ITool tool)
        {
            var probe = tool.Position;
            var ray = new Ray(probe, new Vector3(0, 0, 1));
            var success = UnityEngine.Physics.Raycast(ray, out var hit, 10);
            if (success)
            {
                var verts = CurrentMesh.vertices;
                var trigs = CurrentMesh.triangles;

                var trig = hit.triangleIndex;
                for (int i = 0; i < 3; i++)
                {
                    var index = (trig * 3) + i;
                    var v1 = trigs[index];
                    verts[v1] = Vector3.zero;
                }
            }
        }

        public void Remove(int index, Vector3 offset)
        {
            var arr = CurrentMesh.vertices;
            arr[index] = arr[index] - offset;
            CurrentMesh.SetVertices(arr);
        }
        
        public void Remove(int index)
        {
            var arr = CurrentMesh.vertices;
            arr[index] = Vector3.zero;
            CurrentMesh.SetVertices(arr);
        }

        public void RemoveByTriangle(int index)
        {
             
        }
    }
}