using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class BaseSimulation
    {
        public List<Vector3> Points { get; set; }
        public Color LineColor { get; set; }
        public Vector3 BasePosition { get; }
        
        public virtual void Next(Vector3 point, float durationSeconds = 100)
        {
            Vector3 current;
            if (Points.Count == 0)
            {
                current = BasePosition;
            }
            else
            {
                current = Points.Last();
            }

            Debug.DrawLine(current, point, LineColor, durationSeconds);
        } 
    }
}