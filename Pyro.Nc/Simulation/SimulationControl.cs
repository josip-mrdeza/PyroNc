
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class SimulationControl : MachineComponent
{
    public MovementType Movement { get; set; }
    public UnitType Unit { get; set; }
    
    public void ResetSimulation()
    {
        var controller = Globals.Workpiece;
        controller.ResetColors();
        controller.ResetVertices();
        var machine = MachineBase.CurrentMachine;
        machine.ChangeTool(0);
        machine.ToolControl.SelectedTool.Position = Globals.ReferencePointParser.BeginPoint;
        machine.StateControl.ResetControl();
        ResetSpindle();
        SoftResetCodeSimulation();
        machine.SetTrans(Vector3.zero);
        machine.Runner.Queue.Clear();
    }

    public void SoftResetCodeSimulation()
    {
        MCALL.ClearSubroutine();
        DEF.ClearVariableMap();
        Machine.ToolControl.SelectedTool.Renderer.positionCount = 0;
    }

    public void ResetSpindle()
    {
        Machine.SetFeedRate(0);
        Machine.SetSpindleSpeed(0);
    }
}