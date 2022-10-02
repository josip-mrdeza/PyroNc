using System;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    [Serializable]
    public class ToolConfiguration
    {
        public float radius;
        public int index;
        public Color toolColor;

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public int Index
        {
            get => index;
            set => index = value;
        }

        public Color ToolColor
        {
            get => toolColor;
            set => toolColor = value;
        }

        public ToolConfiguration(float radius, int index, Color toolColor)
        {
            Radius = radius;
            Index = index;
            ToolColor = toolColor;
        }
    }
}