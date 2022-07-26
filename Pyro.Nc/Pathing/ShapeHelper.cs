using System.Linq;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    public static class ShapeHelper
    {
        public static Vector3[] ToVector3s(this I3DShape shape)
        {
            return shape.Points.Select(x => x.ToVector3()).ToArray();
        }
    }
}