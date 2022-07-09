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
    }
}