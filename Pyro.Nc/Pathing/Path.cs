using System.Linq;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    public class Path
    {
        public Vector3[] Points { get; set; }
        public bool Expired { get; set; }
        public Path(Vector3[] points)
        {
            Points = points;
        }

        public override string ToString() => $"{Points.First().ToString()}->{Points.Last().ToString()}";
    }
}