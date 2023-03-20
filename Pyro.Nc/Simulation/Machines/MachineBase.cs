using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Pathing;
using Pyro.Nc.Serializable;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Nc.UI.Entry;
using Pyro.Net;
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
    public NetworkEvent JogEvent { get; private set; }
    
    [StoreAsJson]
    public static bool Test { get; set; }
    [StoreAsJson]
    public static string SimulationId { get; set; }
    
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
        if (ToolControl.SelectedTool == null)
        {
            Push("~[MachineBase]: ToolControl.SelectedTool is null!~");
        }
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
        if (EntryHandler.User == null)
        {
            Push("~[MillMachine]: User has not been logged in, can't listen to events!~");
        }
        else
        {
            SimulationId = EntryHandler.User.Name;
            JogEvent = NetworkEvent.ListenToEvent($"{SimulationId}_jog", "null");
            JogEvent.OnEvent += OnRemoteJog;
            Push($"Listening for jog events on: {JogEvent.Id} [{JogEvent.IsActive}]");
        }
    }

    private void OnRemoteJog(object o, NetworkEventArgs args)
    {
        Queue.Run(async arg =>
        {                                            
            var v3Str = arg.StringData.Value;
            var v3Split = v3Str.Split(',');
            Vector3 v3 = new Vector3((float) v3Split[0].ParseNumber(), (float) v3Split[1].ParseNumber(), (float) v3Split[2].ParseNumber());
            var pos = CurrentMachine.ToolControl.SelectedTool.Position;
            var totalMove = v3;
            var destination = pos + totalMove;
            CurrentMachine.ToolControl.SelectedTool.Position = destination;
            CurrentMachine.Push($"[REMOTE-JOG] - Jogging to {destination.ToString()}...");
        }, args);
    }
    
    private void LateUpdate()
    {
        if (Queue == null)
        {
            return;
        }
        Queue.ThreadHandle().GetAwaiter().GetResult();
    }

    public void ChangeTool(ToolConfiguration configuration)
    {
        ToolControl.SelectedTool.ToolConfig = configuration;
        var id = configuration.Id;
        var mesh = Resources.Load<Mesh>($"Tools/{id}");
        if (mesh == null)
        {
            LocalRoaming lr = LocalRoaming.OpenOrCreate("PyroNc\\CustomTools");
            if (lr.Exists(configuration.Id) || lr.Exists($"{configuration.Id}.obj"))
            {
                var txt = lr.ReadFileAsText(configuration.Id);
                if (string.IsNullOrEmpty(txt))
                {
                    txt = lr.ReadFileAsText($"{configuration.Id}.obj");
                }

                using SerializableMesh sm = SerializableMesh.CreateFromObjText(txt);
                mesh = sm.ToMesh();
            }
        }

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
        if (rpm > 360)
        {
            rpm = 360;
        }
        ToolControl.SelectedTool.Body.angularVelocity = new Vector3(0, rpm * 0.10472f, 0);
        EventSystem.SpindleSpeedChanged();
    }

    public void SetTrans(Vector3 trans)
    {
        SimControl.Trans = trans;
        EventSystem.TransChanged();
    }

    public void SetPosition(Vector3 position)
    {
        ToolControl.SelectedTool.Position = position;
        EventSystem.PositionChanged();
    }
}