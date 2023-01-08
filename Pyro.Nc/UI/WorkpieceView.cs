using System.Globalization;
using Pyro.IO;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.Options;
using Pyro.Nc.UI.UI_Screen;
using UnityEngine;

namespace Pyro.Nc.UI;

public class WorkpieceView : View
{
    public InputReader Length;
    public InputReader Width;
    public InputReader Height;
    public Transform Workpiece;

    public override void Initialize()
    {
        base.Initialize();
        Globals.Tool.Workpiece.View = this;
        Length.OnChanged += OnLengthChanged;
        Width.OnChanged += OnWidthChanged;
        Height.OnChanged += OnHeightChanged;
        var scale = Workpiece.localScale;
        Length.Text = (scale.x * 100).ToString(CultureInfo.InvariantCulture);
        Width.Text = (scale.y * 100).ToString(CultureInfo.InvariantCulture);
        Height.Text = (scale.z * 100).ToString(CultureInfo.InvariantCulture);
    }

    private void OnLengthChanged(string s)
    {
        if (float.TryParse(s, out float x))
        {
            x *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(x, scale.y, scale.z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(x * 50, pos.y, pos.z);
        }
    }
    
    private void OnWidthChanged(string s)
    {
        if (float.TryParse(s, out float z))
        {
            z *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(scale.x, scale.y, z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(pos.x, pos.y, z * 50);
        }
    }
    
    private void OnHeightChanged(string s)
    {
        if (float.TryParse(s, out float y))
        {
            y *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(scale.x, y, scale.z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(pos.x, y * 30, pos.z);
        }
    }
}