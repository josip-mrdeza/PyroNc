using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Simulation.Machines;

public class SystemVariableStorage : MachineComponent
{
    public readonly List<SystemVariable> Variables = new List<SystemVariable>();
    public int Count { get; private set; }
    private ToolBase _toolBase => Machine.ToolControl.SelectedTool;
    private Stopwatch _stopwatch = Stopwatch.StartNew();
    public SystemVariableStorage()
    {
        //Elapsed
        Variables.Add(new SystemVariable(3001, () => (int) _stopwatch.Elapsed.TotalMilliseconds));
        //DTO
        Variables.Add(new SystemVariable(3011, () =>
        {
            var dt = DateTime.Now;
            return float.Parse($"{dt.Year}{dt.Month}{dt.Day}");
        }));
        Variables.Add(new SystemVariable(3012, () =>
        {
            var dt = DateTime.Now;
            return float.Parse($"{dt.Hour}{dt.Minute}{dt.Second}");
        }));
        //CURRENT POSITION
        Variables.Add(new SystemVariable(5021, () => GetPos()[0]));
        Variables.Add(new SystemVariable(5022, () => GetPos()[2]));
        Variables.Add(new SystemVariable(5023, () => GetPos()[1]));
        //TOOL LENGTH
        Variables.Add(new SystemVariable(5081, () => GetLengthOffset()[0]));
        Variables.Add(new SystemVariable(5082, () => GetLengthOffset()[2]));
        Variables.Add(new SystemVariable(5083, () => GetLengthOffset()[1]));
        //SPINDLE
        Variables.Add(new SystemVariable(6000, GetFeed));
        Variables.Add(new SystemVariable(6001, GetSpindleSpeed));
        Variables.Add(new SystemVariable(6002, GetToolNumber));
        Count = Variables.Count;
    }

    private float GetFeed() => Machine.SpindleControl.FeedRate;
    private float GetSpindleSpeed() => Machine.SpindleControl.SpindleSpeed;
    private float GetToolNumber() => Machine.ToolControl.SelectedTool.ToolConfig.Index;
    private Vector3 GetPos() => _toolBase.Position;
    private Vector3 GetLengthOffset() => new Vector3(0,0, _toolBase.ToolConfig.ToolLength);
}

public class SystemVariable
{
    public int Id { get; }
    public Func<float> Value { get; }

    public SystemVariable(int id, Func<float> value)
    {
        Id = id;
        Value = value;
    }
}