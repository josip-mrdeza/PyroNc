using Pyro.Math;
using Pyro.Nc.Configuration;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands;

public struct WorkOffset
{
    public string ID { get; set; }
    public Vector3D Value { get; set; }

    public WorkOffset(string id, Vector3D value)
    {
        ID = id;
        Value = value;
    }

    public override string ToString() => $"({Value.x.ToString()}, {Value.y.ToString()}, {Value.z.ToString()})";

    public static implicit operator Vector3(WorkOffset offset)
    {
        return offset.Value.SwitchYZ().ToVector3();
    }
}