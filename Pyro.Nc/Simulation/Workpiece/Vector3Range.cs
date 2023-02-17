using UnityEngine;

namespace Pyro.Nc.Simulation.Workpiece;

public readonly struct Vector3Range
{
    public readonly Vector3 Start;
    public readonly Vector3 End;

    public Vector3Range(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }
}