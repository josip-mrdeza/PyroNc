using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Configuration.Sim3D_Legacy;

public class RustWrapper
{
    public Vector3 position;
    public Vector3 trV;
    public Vector3 vert;
    public Vector3 realVert;
    public float distanceHorizontal;
    public float radius;
    public CutResult result;
    public Vector3[] vertices;

    public RustWrapper(Vector3 position, Vector3 trV, Vector3 vert,
        Vector3 realVert, float radius, CutResult result, Vector3[] vertices)
    {
        this.position = position;
        this.trV = trV;
        this.vert = vert;
        this.realVert = realVert;
        this.radius = radius;
        this.result = result;
        this.vertices = vertices;
    }
}