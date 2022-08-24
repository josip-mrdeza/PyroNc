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

        public static Triangle[] ToTriangles(this int[] arr)
        {
            Triangle[] tarr = new Triangle[arr.Length / 3];
            for (int i = 0; i < tarr.Length; i++)
            {
                var tr = tarr[i] = new Triangle(0, 0, 0);
                for (int j = 0; j < 3; j++)
                {
                    var index = (i * 3) + j;
                    var val = arr[index];
                    switch (j)
                    {
                        case 0:
                        {
                            tr.A = val;
                            break;
                        }
                        case 1:
                        {
                            tr.B = val;
                            break;
                        }
                        case 2:
                        {
                            tr.C = val;
                            break;
                        }
                    }
                }
            }

            return tarr;
        }
    }
}