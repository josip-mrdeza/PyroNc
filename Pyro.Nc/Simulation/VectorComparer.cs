using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class VectorComparer : IComparer<Vector3>
    {
        public int Compare(Vector3 x, Vector3 y)
        {
            return Vector3.Distance(x, y) < 0.1f ? 0 : 1;
        }
    }
}