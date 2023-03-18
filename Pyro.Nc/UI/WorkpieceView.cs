using System.Globalization;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Nc.UI.Options;
using Pyro.Nc.UI.UI_Screen;
using TinyClient;
using UnityEngine;

namespace Pyro.Nc.UI;

public class WorkpieceView : View
{
    public InputReader Length;
    public InputReader Width;
    public InputReader Height;
    public Transform Workpiece;

    [StoreAsJson]
    public static Vector3D Scale { get; set; }

    public override void Initialize()
    {
        base.Initialize();
        MachineBase.CurrentMachine.Workpiece.View = this;
        Vector3 scale;
        scale = new Vector3(Scale.x, Scale.z, Scale.y);
        Workpiece.localScale = scale / 100f;
        Length.OnChanged += OnLengthChanged;
        Width.OnChanged += OnWidthChanged;
        Height.OnChanged += OnHeightChanged;
        Length.Text = (scale.x).ToString(CultureInfo.InvariantCulture);
        Width.Text = (scale.z).ToString(CultureInfo.InvariantCulture);
        Height.Text = (scale.y).ToString(CultureInfo.InvariantCulture);
        Length.InvokeEvent(Length.Text);
        Width.InvokeEvent(Width.Text);
        Height.InvokeEvent(Height.Text);
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
            workpiece.GenerateVertexBoxHashes(WorkpieceControl.Step, HashmapGenerationReason.WorkpieceLengthChanged);
            UpdateJsonScale(scale.x * 100, 0);
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
            workpiece.GenerateVertexBoxHashes(WorkpieceControl.Step, HashmapGenerationReason.WorkpieceWidthChanged);
            UpdateJsonScale(scale.z * 100, 2);
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
            Workpiece.position = new Vector3(pos.x, y * 50, pos.z);
            workpiece.GenerateVertexBoxHashes(WorkpieceControl.Step, HashmapGenerationReason.WorkpieceHeightChanged);
            UpdateJsonScale(scale.y * 100, 1);
        }
    }

    private void UpdateJsonScale(float val, int key)
    {
        switch (key)
        {
            case 0:
            {
                var sc = Scale;
                sc.x = val;
                Scale = sc;
                break;
            }
            case 1:
            {
                var sc = Scale;
                sc.z = val;
                Scale = sc;
                break;
            }
            case 2:
            {
                var sc = Scale;
                sc.y = val;
                Scale = sc;
                break;
            }
        }
        LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Json");
        roaming.ModifyFile("WorkpieceView.json", new []{new StoreJsonPart(nameof(Scale), Scale)});
    }
}