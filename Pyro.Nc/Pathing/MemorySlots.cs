using System.Collections.Generic;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    public static class MemorySlots
    {
        public static List<Vector3> Saved = new List<Vector3>();

        public static void Add(Vector3 pos)
        {
            Saved.Add(pos);
        }
        
        public static Vector3 Get(int index)
        {
            return Saved[index];
        }
        
        public static void Update(int index, Vector3 pos)
        {
            Saved[index] = pos;
        }
    }
}