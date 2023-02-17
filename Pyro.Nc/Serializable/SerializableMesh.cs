using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Vertices = mesh.vertices.ToArray();
            UVs = mesh.uv;
        }

        public SerializableMesh()
        {
            
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(Vertices.ToArray());
            mesh.SetTriangles(Triangles, 0);
            mesh.SetUVs(0, UVs);
            return mesh;
        }

        public static SerializableMesh CreateFromObj(string fileName)
        {
            return CreateFromObjLines(File.ReadAllLines(fileName));
        }

        public static SerializableMesh CreateFromObjLines(string[] text)
        {
            List<Vector3> vector3s = new List<Vector3>();
            List<int> trigs = new List<int>();
            foreach (var line in text)
            {
                var splitLine = line.Split(' ');

                switch (splitLine[0])
                {
                    case "v":
                    {
                        Vector3 v = new Vector3();
                        v.x = float.Parse(splitLine[1]);
                        v.y = float.Parse(splitLine[2]);
                        v.z = float.Parse(splitLine[3]);
                        vector3s.Add(v);
                        break;
                    }
                    
                    case "f":
                    {
                        trigs.Add(int.Parse(splitLine[1]) - 1);
                        trigs.Add(int.Parse(splitLine[2]) - 1);
                        trigs.Add(int.Parse(splitLine[3]) - 1);
                        //reverse
                        trigs.Add(int.Parse(splitLine[3]) - 1);
                        trigs.Add(int.Parse(splitLine[2]) - 1);
                        trigs.Add(int.Parse(splitLine[1]) - 1);
                        break;
                    }
                }
            }

            var vs = vector3s.ToArray();
            return new SerializableMesh()
            {
                Vertices = vs,
                UVs = vs.Select(x => (Vector2) x).ToArray(),
                Triangles = trigs.ToArray()
            };
        }
        public static SerializableMesh CreateFromObjText(string text)
        {
            return CreateFromObjLines(text.Split('\n'));
        }
    }
}