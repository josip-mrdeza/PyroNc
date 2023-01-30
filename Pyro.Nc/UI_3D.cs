using System;
using System.Globalization;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc;

public class UI_3D : InitializerRoot
{
    public ValueDisplayer FeedRate;
    public ValueDisplayer SpindleSpeed;
    public ValueDisplayer Position;
    public ValueDisplayer Trans;

    public ValueDisplayer Time;

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
        Time = tr.Find("Time_Display").GetComponent<ValueDisplayer>();
        Globals.Tool.OnPositionChanged += SetPositionDisplay;
        Globals.Tool.OnTransChanged += SetTransDisplay;
        Globals.Tool.OnFeedRateChanged += SetFeedDisplay;
        Globals.Tool.OnSpindleSpeedChanged += SetSpindleDisplay;
    }
    
    public void SetPositionDisplay(Vector3 v)
    {
        var valuet = Globals.Tool.Values.IsImperial ? "in" : "mm";
        Position.Value.text = 
            $"X = {v.x.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nY = {v.z.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nZ = {v.y.Round().ToString(CultureInfo.InvariantCulture)}{valuet}";          
    }
    
    public void SetTransDisplay(Vector3 v)
    {
        var valuet = Globals.Tool.Values.IsImperial ? "in" : "mm";
        Trans.Value.text = 
            $"X = {v.x.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nY = {v.z.Round().ToString(CultureInfo.InvariantCulture)}{valuet}" +
            $"\nZ = {v.y.Round().ToString(CultureInfo.InvariantCulture)}{valuet}";
    }
    
    public void SetFeedDisplay(float feed)
    {
        FeedRate.Value.text = $"{feed.ToString(CultureInfo.InvariantCulture)} {(Globals.Tool.Values.IsImperial ? "in/min" : "mm/min")}";
    }
    
    public void SetSpindleDisplay(float rpm)
    {
        SpindleSpeed.Value.text = $"{rpm.ToString(CultureInfo.InvariantCulture)} rpm";
    }

    public void SetTimeDisplay(TimeSpan ts)
    {
        Time.Value.text = ts.ToString();
    }
}