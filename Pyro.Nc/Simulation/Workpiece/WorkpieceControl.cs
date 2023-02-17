using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Serializable;
using Pyro.Nc.UI;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pyro.Nc.Simulation.Workpiece;

public class WorkpieceControl : InitializerRoot
{
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

    public List<Color> Colors
    {
        get
        {
            return _colors;
        }
    }

    public List<Vector3> OriginalVertices { get; private set; }
    public List<Color> OriginalColors { get; private set; }
    public Vector3 MaxValues;
    public Vector3 MinValues;

    private Dictionary<Vector3Range, List<Algorithms.VertexMap>> _vertexBoxHash;
    private Mesh _current;
    [SerializeField] private MeshFilter _currentMeshFilter;
    private SerializableMesh _userAddedMesh;
    private List<Vector3> _vertices;
    private List<Color> _colors;

    [StoreAsJson(nameof(step), typeof(float))]
    internal float step;

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
            _current = mesh;
        }
        OriginalVertices = new List<Vector3>();
        OriginalColors = new List<Color>();
        _userAddedMesh = new SerializableMesh(_current);
        _vertices = new List<Vector3>();
        _colors = new List<Color>();
        _vertexBoxHash = new Dictionary<Vector3Range, List<Algorithms.VertexMap>>(_vertices.Count);
        _current.GetVertices(_vertices);
        _current.GetColors(_colors);
        _current.GetVertices(OriginalVertices);
        _current.GetColors(OriginalColors);
        GenerateVertexBoxHashes(step);
        Globals.Workpiece = this;
    }

    public void ResetVertices()
    {
        Vertices.Clear();
        Vertices.AddRange(OriginalVertices);
    }

    public void ResetColors()
    {
        Colors.Clear();
        Colors.AddRange(OriginalColors);
    }

    public void UpdateVertices()
    {
        _current.SetVertices(_vertices);
        GenerateVertexBoxHashes(step);
    }

    public void UpdateVertices(Vector3[] vertices)
    {
        _current.SetVertices(vertices);
        _current.GetVertices(_vertices);
        GenerateVertexBoxHashes(step);
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
    public void GenerateVertexBoxHashes(float step)
    {
        var maxs = Vertices.Max(v1 => v1.x, v2 => v2.y, v3 => v3.z).ToArray();
        var xx = maxs[0];
        var yy = maxs[1];
        var zz = maxs[2];
        var xCount = (int)System.Math.Ceiling(xx / step);
        var zCount = (int)System.Math.Ceiling(yy / step);
        var yCount = (int)System.Math.Ceiling(zz / step);
        MaxValues = new Vector3(xx, yy, zz);
        MinValues = new Vector3();
        var vertCount = _vertices.Count;
        for (int i = 0; i < vertCount; i++)
        {
            var currentVert = _vertices[i];
            var currentVertexMap = new Algorithms.VertexMap(currentVert, i);
            var xVal = currentVert.x;
            var yVal = currentVert.y;
            var zVal = currentVert.z;
            for (int x = 0; x < xCount; x++)
            {
                var xmin = x * step;
                var xmax = (x + 1) * step;
                bool isInMargin = xVal >= xmin && xVal <= xmax;
                if (!isInMargin)
                {
                    continue;
                }
                for (int z = 0; z < zCount; z++)
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
    }
}