using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Events;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Threading;
using TinyClient;
using UnityEngine;

namespace Pyro.Nc.Simulation.Machines;

public class MachineBase : InitializerRoot
{
    public static MachineBase CurrentMachine { get; private set; }
    public MachineEventSystem EventSystem { get; private set; }
    public MachineSpindleControl SpindleControl { get; private set; }
    public ToolControl ToolControl { get; private set; }
    public WorkpieceControl Workpiece { get; private set; }
    public Executor Runner { get; private set; }
    public MachineStateControl StateControl { get; private set; }
    public SimulationControl SimControl { get; private set; }
    public ClickHandler ClickHandler => ClickHandler.Instance;
    public MachineType CncType { get; protected set; }
    public ThreadTaskQueue Queue { get; private set; }

    [StoreAsJson]
    public static CutType CuttingType { get; set; }
    [StoreAsJson]
    public static bool Test { get; set; }
    
    public override void Initialize()
    {
        CurrentMachine = this;
        Queue = new ThreadTaskQueue();
        EventSystem = new MachineEventSystem();
        SpindleControl = new MachineSpindleControl();
        Workpiece = Globals.Workpiece;
        Runner = new Executor();
        StateControl = new MachineStateControl();
        SimControl = new SimulationControl();
        ToolControl = gameObject.AddComponent<ToolControl>();
        ToolControl.SelectedTool.Initialize();
        SpindleControl.FeedRate.SetUpperValue(400);
        SpindleControl.SpindleSpeed.SetUpperValue(4000);
        ClickHandler.OnControlDoubleClick += async v =>
        {
            Push($"Jogging to: {v.ToString()}!");
            await Runner.Jog(v);
        };
        SpindleControl.FeedRate.SetUpperValue(350);
        SpindleControl.SpindleSpeed.SetUpperValue(4000);
    }

    private void LateUpdate()
    {
        Queue.ThreadHandle().GetAwaiter().GetResult();
    }

    public void ChangeTool(ToolConfiguration configuration)
    {
        ToolControl.SelectedTool.ToolConfig = configuration;
        var id = configuration.Id;
        var mesh = Resources.Load<Mesh>($"Tools/{id}");
        ToolControl.Filter.mesh = mesh;
        EventSystem.ToolChanged();
    }

    public void ChangeTool(float radius)
    {
        var tool = ToolControl.Manager.Tools.Find(x => x.Radius == radius);
        ChangeTool(tool);
    }

    public void SetFeedRate(float feed)
    {
        SpindleControl.FeedRate.Set(feed);
        EventSystem.FeedRateChanged();
    }

    public void SetSpindleSpeed(float rpm)
    {
        SpindleControl.SpindleSpeed.Set(rpm);
        ToolControl.SelectedTool.Body.angularVelocity = new Vector3(0, rpm * 0.10472f, 0);
        EventSystem.SpindleSpeedChanged();
    }

    public void SetTrans(Vector3 trans)
    {
        ToolControl.SelectedTool.Values.TransPosition = trans;
        EventSystem.TransChanged();
    }

    public void SetPosition(Vector3 position)
    {
        ToolControl.SelectedTool.Position = position;
        EventSystem.PositionChanged();
    }
}