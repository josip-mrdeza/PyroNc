using System.Globalization;
using Pyro.IO;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
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
        MachineBase.CurrentMachine.Workpiece.View = this;
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
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            ref var max = ref workpiece.MaxValues;
            max.x = x;
            x *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(x, scale.y, scale.z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(x * 50, pos.y, pos.z);
            workpiece.GenerateVertexBoxHashes(workpiece.step);
        }
    }
    
    private void OnWidthChanged(string s)
    {
        if (float.TryParse(s, out float z))
        {
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            ref var max = ref workpiece.MaxValues;
            max.z = z;
            z *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(scale.x, scale.y, z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(pos.x, pos.y, z * 50);
            workpiece.GenerateVertexBoxHashes(workpiece.step);
        }
    }
    
    private void OnHeightChanged(string s)
    {
        if (float.TryParse(s, out float y))
        {
            var workpiece = MachineBase.CurrentMachine.Workpiece;
            ref var max = ref workpiece.MaxValues;
            max.y = y;          
            y *= 0.01f;
            var scale = Workpiece.localScale;
            Workpiece.localScale = new Vector3(scale.x, y, scale.z);
            var pos = Workpiece.position;
            Workpiece.position = new Vector3(pos.x, y * 30, pos.z);
            workpiece.GenerateVertexBoxHashes(workpiece.step);
        }
    }
}