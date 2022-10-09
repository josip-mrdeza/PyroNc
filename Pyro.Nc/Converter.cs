using Pyro.Math;
using UnityEngine;

namespace Pyro.Nc
{
    public static class Converter
    {
        public static Vector3 ToVector3(this Vector3D vector3D)
        {
            return new Vector3(vector3D.x, vector3D.y, vector3D.z);
        }

        public static Vector3D ToVector3D(this Vector3 vector3)
        {
            return new Vector3D(vector3.x, vector3.y, vector3.z);
        }

        public static Vector3 RemoveNan(this Vector3 vector3)
        {
            return new Vector3(float.IsNaN(vector3.x) ? 0 : vector3.x,
                               float.IsNaN(vector3.y) ? 0 : vector3.y,
                               float.IsNaN(vector3.z) ? 0 : vector3.z);
        }
    }
}