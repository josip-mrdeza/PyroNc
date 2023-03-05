

using Pyro.Math;
using UnityEngine;

namespace Pyro.Nc.Configuration;

public static class SwitchHelper
{
    public static Vector3 SwitchYZ(this Vector3 v3)
    {
        (v3.y, v3.z) = (v3.z, v3.y);

        return v3;
    }

    public static Vector3D SwitchYZ(this Vector3D v3)
    {
        (v3.y, v3.z) = (v3.z, v3.y);

        return v3;
    }
}