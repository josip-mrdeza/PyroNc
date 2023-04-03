
using System;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.SyntacticalCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class SimulationControl : MachineComponent
{
    public MovementType Movement { get; set; }
    public UnitType Unit { get; set; }
    public Vector3 Trans { get; set; }
    public WorkOffset WorkOffset { get; set; }

    [StoreAsJson]
    [Obsolete]
    public static WorkOffset[] workOffsets { get; set; } = new WorkOffset[4]
    {
        new WorkOffset("G54", new Vector3D()),
        new WorkOffset("G55", new Vector3D()),
        new WorkOffset("G56", new Vector3D()),
        new WorkOffset("G57", new Vector3D())
    };

    public WorkOffset[] WorkOffsets
    {
        get => workOffsets;
        set => workOffsets = value;
    }
    public bool IsInCuttingMode
    {
        get
        {
            var currentContext = Machine.Runner.CurrentContext;
            if (currentContext is FORLOOP loop)
            {
                return loop.CurrentLoopContext is G01;
            }

            if (currentContext is Cycle cycle)
            {
                if (cycle.CurrentContext is G01)
                {
                    return true;
                }
            }
            return currentContext is G01;
        }
    }
    
    public async void ResetSimulation()
    {
        var machine = MachineBase.CurrentMachine;
        machine.StateControl.ResetControl();
        var controller = Globals.Workpiece;
        controller.ResetColors();
        controller.ResetVertices();
        machine.ChangeTool(0);
        machine.ToolControl.SelectedTool.Position = Globals.ReferencePointParser.BeginPoint;
        machine.StateControl.ResetUI();
        ResetSpindle();
        SoftResetCodeSimulation();
        machine.SetTrans(Vector3.zero);
    }

    public void SoftResetCodeSimulation()
    {
        MCALL.ClearSubroutine();
        DEF.ClearVariableMap();
        Machine.ToolControl.SelectedTool.Renderer.positionCount = 0;
        Machine.SimControl.WorkOffset = new WorkOffset();
    }

    public void ResetSpindle()
    {
        Machine.SetFeedRate(0);
        Machine.SetSpindleSpeed(0);
    }
}