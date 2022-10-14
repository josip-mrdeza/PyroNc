using System;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class ToolConfiguration
    {
        public string Name { get; set; }
        public float Radius { get; set; }
        public int Index { get; set; }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Color ToolColor;
        
        [JsonConstructor]
        public ToolConfiguration(string name, float radius, int index, float r, float g, float b, float a)
        {
            Name = name;
            Radius = radius;
            Index = index;
            ToolColor = new Color(r /= 255, g /= 255, b /= 255, a /= 255);
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        internal ToolConfiguration(string name, float radius, int index, Color color)
        {
            Name = name;
            Radius = radius;
            Index = index;
            ToolColor = color;
            R = color.r * 255;
            G = color.g * 255;
            B = color.b * 255;
            A = color.a * 255;
        }
    }
}