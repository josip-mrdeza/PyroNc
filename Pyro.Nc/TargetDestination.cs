using UnityEngine;

namespace Pyro.Nc
{
    public struct TargetDestination
    {
        public bool IsValid;
        public bool HasArrived;
        public Vector3 Location;

        public TargetDestination(Vector3 location)
        {
            IsValid = true;
            HasArrived = false;
            Location = location;
        }

        public void MarkArrived()
        {
            HasArrived = true;
        }
    }
}