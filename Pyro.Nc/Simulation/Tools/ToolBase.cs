using System;
using Pyro.IO.Events;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation.Tools;

public class ToolBase : InitializerRoot
{
    public ToolValues Values { get; protected set; }
    public ToolConfiguration ToolConfig { get; set; }
    public Rigidbody Body { get; private set; }
    public Transform Transform => transform;
    public Vector3 Position
    {
        get => _transform.position;
        set
        {
            MachineBase.CurrentMachine.SetPosition(value);
        }
    }
    private Transform _transform;

    public override void Initialize()
    {
        Body = GetComponent<Rigidbody>();
        _transform = gameObject.transform;
        Globals.Tool = this;
    }
}