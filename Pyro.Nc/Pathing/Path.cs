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
    }
}