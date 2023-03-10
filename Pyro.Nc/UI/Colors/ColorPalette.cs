using System;
using Pyro.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Colors;

public class ColorPalette : MonoBehaviour
{
    [StoreAsJson]
    public static PyroColor Palette { get; set; }
    [StoreAsJson]
    public static bool ShouldUsePalette { get; set; }
    private void Start()
    {
        if (!ShouldUsePalette)
        {
            return;
        }
        var gos = gameObject.scene.GetRootGameObjects();
        foreach (var go in gos)
        {
            Image[] imgs = go.GetComponentsInChildren<Image>();
            foreach (var image in imgs)
            {
                ApplyColorPalette(image);
            }
        }
    }

    public void ApplyColorPalette(Image img)
    {
        img.color = Palette;
    }
}

public struct PyroColor
{
    public float A { get; set; }
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }

    public PyroColor(float a, float r, float g, float b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public override string ToString() => ((Color) this).ToString();

    public static implicit operator Color(PyroColor color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }
}