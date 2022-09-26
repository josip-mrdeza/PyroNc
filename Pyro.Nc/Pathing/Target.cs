using UnityEngine;

namespace Pyro.Nc.Pathing
{
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
        }

        public override string ToString() => Location.ToString();
    }
}