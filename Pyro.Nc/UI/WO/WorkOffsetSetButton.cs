using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.WO;

public class WorkOffsetSetButton : InitializerRoot
{
    public Button SaveButton;
    public static List<WorkOffsetSetter> Setters;

    public override void Initialize()
    {
        SaveButton = GetComponent<Button>();
        SaveButton.onClick.AddListener(OnClick);
        Setters = new List<WorkOffsetSetter>();
    }

    private void OnClick()
    {
        foreach (var setter in Setters)
        {
            try
            {
                setter.Set();
            }
            catch (Exception e)
            {
                Globals.Console.Push($"~[WorkOffsetSetButton]: {e.Message}!~");
            }
        }
        LocalRoaming lr = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Json");
        var arr = lr.ReadFileAs<StoreJsonPart[]>("SimulationControl.json");
        var part = arr.First(x => x.Name == "workOffsets");
        var offsets = MachineBase.CurrentMachine.SimControl.WorkOffsets;
        part.Value = offsets;
        lr.ModifyFile("SimulationControl.json", arr);                 
    }
}