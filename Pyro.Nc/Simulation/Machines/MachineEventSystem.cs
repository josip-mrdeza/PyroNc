using System;
using Pyro.IO.Events;
using Pyro.Nc.Configuration;
using UnityEngine;

namespace Pyro.Nc.Simulation.Machines;

public class MachineEventSystem : MachineComponent
{
    public event EventHandler<ToolConfiguration> OnToolChanged;
    public event EventHandler<float> OnSpindleSpeedChanged;
    public event EventHandler<float> OnFeedRateChanged;
    public event EventHandler<Vector3> OnTransChanged;
    public event EventHandler<Vector3> OnPositionChanged;
    public PyroEventSystem PEvents { get; }

    public MachineEventSystem()
    {
        PEvents = new PyroEventSystem();
    }
    public void ToolChanged()
    {
        OnToolChanged?.Invoke(null, Machine.ToolControl.SelectedTool.ToolConfig);
        PEvents.Fire(Locals.EventConstants.ToolChange);
    }

    public void SpindleSpeedChanged()
    {
        OnSpindleSpeedChanged?.Invoke(null, Machine.SpindleControl.SpindleSpeed);
        PEvents.Fire(Locals.EventConstants.SpindleSpeedChange);
    }

    public void FeedRateChanged()
    {
        OnFeedRateChanged?.Invoke(null, Machine.SpindleControl.FeedRate);
        PEvents.Fire(Locals.EventConstants.FeedRateChange);
    }

    public void TransChanged()
    {
        OnTransChanged?.Invoke(null, Machine.ToolControl.SelectedTool.Values.TransPosition);
        PEvents.Fire(Locals.EventConstants.TransPositionChange);
    }

    public void PositionChanged()
    {
        var pos = Machine.ToolControl.SelectedTool.Position;
        OnPositionChanged?.Invoke(null, pos);
        PEvents.Fire(Locals.EventConstants.PositionChange, pos);
    }
}