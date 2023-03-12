using System;
using System.Globalization;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.UI;
using TMPro;
using UnityEngine;

namespace Pyro.Nc;

public class UI_3D : InitializerRoot
{
    public ValueDisplayer FeedRate;
    public ValueDisplayer SpindleSpeed;
    public ValueDisplayer Position;
    public ValueDisplayer Trans;

    public TimeValueDisplayer Time;
    public TextMeshProUGUI CodeDisplay;

    private TimeSpan Previous;

    public static UI_3D Instance;

    public override void Initialize()
    {
        Instance = this;
        var tr = transform;
        FeedRate = tr.Find("FeedRate_Display").GetComponent<ValueDisplayer>();
        SpindleSpeed = tr.Find("SpindleSpeed_Display").GetComponent<ValueDisplayer>();
        Position = tr.Find("Position_Display").GetComponent<ValueDisplayer>();
        Trans = tr.Find("Trans_Display").GetComponent<ValueDisplayer>();
        Time = tr.Find("Time_Display").GetComponent<TimeValueDisplayer>();
        CodeDisplay = tr.Find("GCode_Displayer").GetComponent<TextMeshProUGUI>();
        var mach = MachineBase.CurrentMachine;
        var machineEventSystem = mach.EventSystem;
        machineEventSystem.OnPositionChanged += (_, pos)=> SetPositionDisplay(pos);
        machineEventSystem.OnTransChanged += (_, t) => SetTransDisplay(t);
        machineEventSystem.OnFeedRateChanged += (_, f) => SetFeedDisplay(f);
        machineEventSystem.OnSpindleSpeedChanged += (_, s) => SetSpindleDisplay(s);
        machineEventSystem.PositionChanged();
        machineEventSystem.TransChanged();
        machineEventSystem.FeedRateChanged();
        machineEventSystem.SpindleSpeedChanged();
    }

    public void SetMessage(string s)
    {
        CodeDisplay.text = s;
    }
    
    public void SetPositionDisplay(Vector3 v)
    {
        var valuet = MachineBase.CurrentMachine.SimControl.Unit == UnitType.Imperial ? "in" : "mm";
        Position.Value.text = 
            $"X = {v.x.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nY = {v.z.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nZ = {v.y.Round().ToString(CultureInfo.InvariantCulture)}{valuet}";          
    }
    
    public void SetTransDisplay(Vector3 v)
    {
        var valuet = MachineBase.CurrentMachine.SimControl.Unit == UnitType.Imperial ? "in" : "mm";
        Trans.Value.text = 
            $"X = {v.x.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nY = {v.z.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nZ = {v.y.Round().ToString(CultureInfo.InvariantCulture)}{valuet}";
    }
    
    public void SetFeedDisplay(float feed)
    {
        FeedRate.Value.text = $"{feed.ToString(CultureInfo.InvariantCulture)} {(MachineBase.CurrentMachine.SimControl.Unit == UnitType.Imperial ? "in/min" : "mm/min")}";
    }
    
    public void SetSpindleDisplay(float rpm)
    {
        SpindleSpeed.Value.text = $"{rpm.ToString(CultureInfo.InvariantCulture)} rpm";
    }

    public void SetTimeDisplay(TimeSpan ts)
    {
        Time.Time = ts;
    }

    public void IncrementTimeDisplay(TimeSpan ts)
    {
        Time.Time += ts;
    }
}