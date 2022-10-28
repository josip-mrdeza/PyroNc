using System;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    [Serializable]
    public class Target
    {
        public bool IsValid;
        public bool HasArrived;
        public Vector3 Location;

        public Target(Vector3 location)
        {
            IsValid = false;
            HasArrived = false;
            Location = location;
        }

        public void MarkArrived()
        {
            HasArrived = true;
            IsValid = false;
        }

        public override string ToString() => Location.ToString();
    }
}