using System;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    [Serializable]
    public class Path
    {
        [SerializeField] private Vector3[] _points;
        [SerializeField] private bool _expired;

        public Vector3[] Points
        {
            get => _points;
            set => _points = value;
        }

        public bool Expired
        {
            get => _expired;
            set => _expired = value;
        }

        public Path(Vector3[] points)
        {
            Points = points;
        }

        public override string ToString() => $"{Points.First().ToString()}->{Points.Last().ToString()}";
    }
}