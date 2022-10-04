using System;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class ToolConfiguration
    {
        public float Radius { get; set; }

        public int Index { get; set; }

        public Color ToolColor { get; set; }

        public ToolConfiguration(float radius, int index, Color toolColor)
        {
            Radius = radius;
            Index = index;
            ToolColor = toolColor;
        }
    }
}