using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Machines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.WO;

public class WorkOffsetSetter : InitializerRoot
{
    public TMP_InputField X;
    public TMP_InputField Y;
    public TMP_InputField Z;

    public override void Initialize()
    {
        var comps = GetComponentsInChildren<TMP_InputField>();
        X = comps[0];
        Y = comps[1];
        Z = comps[2];
        
        WorkOffsetSetButton.Setters.Add(this);
        Load();
    }

    public void Set()
    {
        var wo = new WorkOffset(name, new Vector3D((float) X.text.FixEmptyString<float>().ParseNumber(), 
                                                   (float) Y.text.FixEmptyString<float>().ParseNumber(), 
                                                   (float) Z.text.FixEmptyString<float>().ParseNumber()));
        var arr = MachineBase.CurrentMachine.SimControl.WorkOffsets;
        var goName = name;
        for (int i = 0; i < arr.Length; i++)
        {
            var woCurr = arr[i];
            if (woCurr.ID == goName)
            {
                arr[i] = wo;
                break;
            }
        } 
    }

    public void Load()
    {
        WorkOffset wo = default;
        var arr = MachineBase.CurrentMachine.SimControl.WorkOffsets;
        var goName = name;
        for (int i = 0; i < arr.Length; i++)
        {
            var woCurr = arr[i];
            if (woCurr.ID == goName)
            {
                wo = woCurr;
                break;
            }
        }

        var val = wo.Value;
        if (val.x != 0)
        {
            X.text = val.x.ToString(CultureInfo.InvariantCulture);
        }
        if (val.y != 0)
        {
            Y.text = val.y.ToString(CultureInfo.InvariantCulture);
        }
        if (val.z != 0)
        {
            Z.text = val.z.ToString(CultureInfo.InvariantCulture);
        }
    }
}