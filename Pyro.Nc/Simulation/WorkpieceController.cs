using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EarClipperLib;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Serializable;
using Pyro.Nc.UI.UI_Screen;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class WorkpieceController : InitializerRoot
    {
        public Mesh StartingMesh;
        public MeshFilter Filter;
        public MeshCollider Collider;
        public override async Task InitializeAsync()
        {
            PopupHandler.PopText("balls are nice.");
            if (Globals.Roaming.Exists("Mesh.obj"))
            {
                var sMesh = ParseObj(Globals.Roaming.ReadFileAsText("Mesh.obj"));
                Filter.mesh = sMesh.ToMesh();
                return;
            }

            Filter.mesh = StartingMesh;
            /*if (Globals.Roaming.Exists("Mesh.json"))
            {
                Filter.mesh = Globals.Roaming.ReadFileAs<SerializableMesh>("Mesh.json").ToMesh();
            }
            else
            {
                var jsonMesh = new SerializableMesh(StartingMesh);
                var json = JsonUtility.ToJson(jsonMesh);
                Push("Saved starting mesh to json format: ",
                     "Length: {0}".Format(json.Length),
                     "Vertices: {0}".Format(StartingMesh.vertexCount),
                     "Triangles: {0}".Format(StartingMesh.triangles.Length));
                Globals.Roaming.AddFile("Mesh.json", json);
            }*/
        }

        public static SerializableMesh ParseObj(string objText)
        {
            SerializableMesh mesh = LoadObject(objText.Split('\n'));

            return mesh;
        }

        private static SerializableMesh LoadObject(string[] text)
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
        private static IEnumerable<Vector3> LoadObjFromString(string[] split)
        {
            foreach (var line in split)
            {
                var splitLine = line.Split(' ');
                Vector3 v = new Vector3();
                if (splitLine[0] == "v")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        v[i] = float.Parse(splitLine[i + 1]);
                    }
                    yield return v;
                }
            }
        }
    }
}