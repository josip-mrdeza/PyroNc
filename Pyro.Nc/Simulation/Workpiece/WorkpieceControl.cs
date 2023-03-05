using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Serializable;
using Pyro.Nc.UI;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pyro.Nc.Simulation.Workpiece;

public class WorkpieceControl : InitializerRoot
{
    public static WorkpieceControl Instance;
    public WorkpieceView View;

    public List<Vector3> Vertices
    {
        get
        {
            return _vertices;
        }
        set
        {
            _vertices = value;
            UpdateVertices();
        }
    }

    public int[] Triangles
    {
        get => _current.triangles;
        set
        {
            Push($"[TRIANGLES]: {value.Length} indices, {value.Length / 3f} triangles");
            _current.triangles = value;
        }
    }

    public List<Color> Colors
    {
        get
        {
            return _colors;
        }
    }

    public List<Vector3> OriginalVertices { get; private set; }
    public List<Color> OriginalColors { get; private set; }
    public Dictionary<Vector3Range, List<Algorithms.VertexMap>> VertexBoxHash => _vertexBoxHash;
    public Vector3 MaxValues;
    public Vector3 MinValues;
    //public readonly WorkpieceStatistics Statistics = WorkpieceStatistics.Statistics;

    private Dictionary<Vector3Range, List<Algorithms.VertexMap>> _vertexBoxHash;
    private Mesh _current;
    [SerializeField] private MeshFilter _currentMeshFilter;
    private SerializableMesh _userAddedMesh;
    private List<Vector3> _vertices;
    private List<Color> _colors;

    [StoreAsJson]
    public static float Step { get; set; } = 12.5f;

    public override void Initialize()
    {
        LoadUserAddedMesh();
        if (_current == null)
        {
            _currentMeshFilter = GetComponent<MeshFilter>();
            var mesh = _currentMeshFilter.mesh;
            mesh.name = "Default Pyro Mesh";
            mesh.Optimize();
            mesh.MarkDynamic();
            _current = new Mesh();
            _current.vertices = mesh.vertices;
            _current.colors = mesh.colors;
            _current.triangles = mesh.triangles;
            _current.RecalculateNormals();
            _currentMeshFilter.mesh = _current;
            _currentMeshFilter.sharedMesh = _current;
        }
        _userAddedMesh = new SerializableMesh(_current);
        _vertexBoxHash = new Dictionary<Vector3Range, List<Algorithms.VertexMap>>(200);
        _vertices = _current.vertices.ToList();
        OriginalVertices = _current.vertices.ToList();
        OriginalColors = Enumerable.Repeat(Color.white, _vertices.Count).ToList();
        _colors = Enumerable.Repeat(Color.white, _vertices.Count).ToList();
        var tr = transform;
        var maxs = Vertices.Select(x => tr.TransformPoint(x)).Max(v1 => v1.x, v2 => v2.y, v3 => v3.z).ToArray();
        var xx = maxs[0];
        var yy = maxs[1];
        var zz = maxs[2];
        MaxValues = new Vector3(xx, yy, zz);
        GenerateVertexBoxHashes(Step, HashmapGenerationReason.Initialized);
        Globals.Workpiece = this;
        Instance = this;
    }
                                         
    public void ResetVertices()
    {
        _vertices = OriginalVertices.ToList();
        UpdateVertices();
    }

    public void ResetColors()
    {
        _colors = OriginalColors.ToList();
    }

    public void UpdateVertices()
    {
        _current.vertices = _vertices.GetInternalArray();
        _current.colors = _colors.GetInternalArray();
    }

    public void UpdateVerticesAndGenerateHash(List<Vector3Range> affectedRanges)
    {
        UpdateVertices();
        RegenerateVertexBoxHashes(affectedRanges, Step, HashmapGenerationReason.UpdatedVertices);
    }
    
    public void UpdateVerticesAndGenerateHash(Vector3Range affectedRange)
    {
        UpdateVertices();
        RegenerateVertexBoxHash(affectedRange, Step, HashmapGenerationReason.UpdatedVertices);
        //Statistics.Increment(nameof(UpdateVerticesAndGenerateHash));
    }

    public void UpdateVertices(Vector3[] vertices)
    {
        _current.SetVertices(vertices);
        _current.GetVertices(_vertices);
        GenerateVertexBoxHashes(Step, HashmapGenerationReason.UpdatedVertices);
    }
    private void LoadUserAddedMesh()
    {
        if (!Globals.Roaming.Exists("Mesh.obj"))
        {
            return;
        }
        var sMesh = SerializableMesh.CreateFromObjText(Globals.Roaming.ReadFileAsText("Mesh.obj"));
        var mesh = _currentMeshFilter.mesh = sMesh.ToMesh();
        mesh.name = "customObj";
        _current = mesh;
        _userAddedMesh = sMesh;
    }

    public void RegenerateVertexBoxHashes(List<Vector3Range> affectedRanges, float step, HashmapGenerationReason reason)
    {
        var tr = transform;
        foreach (var range in affectedRanges)
        {
            var list = VertexBoxHash[range];
            for (var i = 0; i < list.Count; i++)
            {
                var map = list[i];
                if (range.Fits(map.Vertex))
                {
                    map.Vertex = tr.TransformPoint(_vertices[map.Index]);
                    list[i] = map;
                }
                else
                {
                    list.Remove(map);
                    for (int j = 0; j < _vertexBoxHash.Count; j++)
                    {
                        var r = _vertexBoxHash.Keys.ElementAt(j);
                        if (r.Fits(map.Vertex))
                        {
                            _vertexBoxHash[r].Add(map);
                        }
                    }
                }
            }
        }
        //DrawHashedBlocks();
        //Statistics.Increment(nameof(RegenerateVertexBoxHashes));
    }
    
    public void RegenerateVertexBoxHash(Vector3Range affectedRange, float step, HashmapGenerationReason reason)
    {
        var tr = transform;
        var list = VertexBoxHash[affectedRange];
        for (var i = 0; i < list.Count; i++)
        {
            var map = list[i];
            if (affectedRange.Fits(map.Vertex))
            {
                map.Vertex = tr.TransformPoint(_vertices[map.Index]);
                list[i] = map;
            }
            else
            {
                list.Remove(map);
                for (int j = 0; j < _vertexBoxHash.Count; j++)
                {
                    var r = _vertexBoxHash.Keys.ElementAt(j);
                    if (r.Fits(map.Vertex))
                    {
                        _vertexBoxHash[r].Add(map);
                    }
                }
            }
        }
        //DrawHashedBlocks();
        //Statistics.Increment(nameof(RegenerateVertexBoxHash));
    }
    public void GenerateVertexBoxHashes(float step, HashmapGenerationReason reason)
    {
        if (step == 0)
        {
            return;
        }
        _vertexBoxHash.Clear();
        var tr = transform;
        var maxs = MaxValues;
        var xx = maxs[0];
        var yy = maxs[1];
        var zz = maxs[2];
        var xCount = (int)System.Math.Ceiling(xx / step);
        var zCount = (int)System.Math.Ceiling(yy / step);
        var yCount = (int)System.Math.Ceiling(zz / step);
        MinValues = new Vector3();
        var vertCount = _vertices.Count;
        for (int i = 0; i < vertCount; i++)
        {
            var currentVert = tr.TransformPoint(_vertices[i]);
            var currentVertexMap = new Algorithms.VertexMap(currentVert, i);
            var xVal = currentVert.x;
            var yVal = currentVert.y;
            var zVal = currentVert.z;
            for (int x = -1; x < xCount; x++)
            {
                var xmin = x * step;
                var xmax = (x + 1) * step;
                bool isInMargin = xVal >= xmin && xVal <= xmax;
                if (!isInMargin)
                {
                    continue;
                }
                for (int z = -1; z < zCount; z++)
                {
                    var zmin = z * step;
                    var zmax = (z + 1) * step;
                    isInMargin = zVal >= zmin && zVal <= zmax;
                    if (!isInMargin)
                    {
                        continue;
                    }

                    for (int y = 0; y < yCount; y++)
                    {
                        var ymin = y * step;
                        var ymax = (y + 1) * step;
                        isInMargin = yVal >= ymin && yVal <= ymax;
                        if (!isInMargin)
                        {
                            continue;
                        }
                        
                        FinalGenerate(
                            new Vector3(xmin, ymin, zmin), 
                            new Vector3(xmax, ymax, zmax),
                            currentVertexMap);
                    }
                }
            }
        }
        #if DEBUG
        var sum = _vertexBoxHash.Values.Sum(x => x.Count);
        var averagePerSide = _vertexBoxHash.Values.Count / 6;

        if (_vertices.Count > sum)
        {
            Globals.Console.Push(
                $"[ALGORITHMS/WORKPIECE]-{nameof(GenerateVertexBoxHashes)}: ({reason.ToString()}) Generation failed, vertex list contains more elements than hashmap " +
                $"*({_vertices.Count}v) > *({sum}v in {_vertexBoxHash.Values.Count} blocks) with an increment of {step}!");
        }
        else
        {
            Globals.Console.Push(
                $"[ALGORITHMS/WORKPIECE]-{nameof(GenerateVertexBoxHashes)}: ({reason.ToString()}) Generation succeeded, vertex list contains less or equal elements than hashmap " +
                $"*({_vertices.Count}v) <= *({sum}v in {_vertexBoxHash.Values.Count} blocks) with an increment of {step}.");
        }
        //DrawHashedBlocks();
        #endif
        //Statistics.Increment(nameof(GenerateVertexBoxHashes));
    }
    private void FinalGenerate(Vector3 minVec, Vector3 maxVec, Algorithms.VertexMap vertexMap)
    {
        var range = new Vector3Range(minVec, maxVec);
        if (_vertexBoxHash.TryGetValue(range, out var list))
        {
            list.Add(vertexMap);
        }
        else
        {
            _vertexBoxHash.Add(range, new List<Algorithms.VertexMap>()
            {
                vertexMap
            });
        }
        //Statistics.Increment(nameof(FinalGenerate));
    }

    public void DrawHashedBlocks()
    {
        foreach (var vhv in _vertexBoxHash)
        {
            var val1 = vhv.Key.Start;
            var val2 = vhv.Key.End;
                    
            //Debug.DrawLine(val1, new Vector3(val2.x, val1.y, val2.z), Color.red, 10000);
            for (var i = 1; i < vhv.Value.Count; i++)
            {
                //Debug.DrawLine(vhv.Value[i-1].Vertex, vhv.Value[i].Vertex, Color.green, 0.5F);
                var v = vhv.Value[i - 1].Vertex;
                Debug.DrawLine(vhv.Value[i-1].Vertex, v + new Vector3(0.2f, 0, 0), Color.green, 0.1F);
            }
        }
        //Statistics.Increment(nameof(DrawHashedBlocks));
    }

    private void Update()
    {
        //DrawHashedBlocks();
    }
}