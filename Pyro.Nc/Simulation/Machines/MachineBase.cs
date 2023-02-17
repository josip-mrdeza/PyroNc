using System;
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
    public MachineType CncType { get; protected set; }
    
    public override void Initialize()
    {
        CurrentMachine = this;
        EventSystem = new MachineEventSystem();
        SpindleControl = new MachineSpindleControl();
        ToolControl = new ToolControl();
        Workpiece = Globals.Workpiece;
        Runner = new Executor();
        StateControl = new MachineStateControl();
        SimControl = new SimulationControl();
        Assembly.GetCallingAssembly().TraverseAssemblyAndCreateJsonFiles(LocalRoaming.OpenOrCreate("PyroNc\\Configuration")).Iterate();
    }
    
    public void ChangeTool(ToolConfiguration configuration)
    {
        ToolControl.SelectedTool.ToolConfig = configuration;
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