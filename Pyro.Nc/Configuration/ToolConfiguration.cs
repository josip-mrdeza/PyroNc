using System;
using System.Linq;
using System.Text.Json.Serialization;
using Pyro.IO;
using Pyro.Nc.Serializable;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class ToolConfiguration
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public float Radius { get; set; }
        public int Index { get; set; }
        public float VerticalMargin { get; set; }
        public float ToolLength { get; set; }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Color _ToolColor;

        public void RefreshColor()
        {
            _ToolColor = new Color(R / 255, G / 255, B / 255, A / 255);
        }

        public void RefreshToolLength()
        {
            var mesh = Resources.Load<Mesh>($"Tools\\{Id}");
            if (mesh == null)
            {
                LocalRoaming lr = LocalRoaming.OpenOrCreate("PyroNc\\CustomTools");
                if (lr.Exists(Id) || lr.Exists($"{Id}.obj"))
                {
                    var txt = lr.ReadFileAsText(Id);
                    if (string.IsNullOrEmpty(txt))
                    {
                        txt = lr.ReadFileAsText($"{Id}.obj");
                    }

                    using SerializableMesh sm = SerializableMesh.CreateFromObjText(txt);
                    mesh = sm.ToMesh();
                }
                var verts = mesh.vertices;
                if (verts.Length == 0)
                {
                    return;
                }
                var max = verts.Max(var => var.z);
                var min = verts.Min(var => var.z);

                var diff = System.Math.Abs(max - min);
                ToolLength = diff;
            }
            else
            {
                var verts = mesh.vertices;
                if (verts.Length == 0)
                {
                    return;
                }
                var max = verts.Max(var => var.z);
                var min = verts.Min(var => var.z);

                var diff = System.Math.Abs(max - min);
                ToolLength = diff;
                Resources.UnloadAsset(mesh);
            }
        }

        public Color GetColor()
        {
            RefreshColor();

            return _ToolColor;
        }
        
        [JsonConstructor]
        public ToolConfiguration(string name,string id, float radius, int index, float verticalMargin, float toolLength, float r, float g, float b, float a)
        {
            Name = name;
            Id = id;
            Radius = radius;
            Index = index;
            _ToolColor = new Color(r / 255, g / 255, b / 255, a / 255);
            VerticalMargin = verticalMargin;
            ToolLength = toolLength;
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        internal ToolConfiguration(string name, string id, float radius, int index, float verticalMargin, float toolLength, Color color)
        {
            Name = name;
            Id = id;
            Radius = radius;
            Index = index;
            _ToolColor = color;
            VerticalMargin = verticalMargin;
            ToolLength = toolLength;
            R = color.r * 255;
            G = color.g * 255;
            B = color.b * 255;
            A = color.a * 255;
        }
    }
}