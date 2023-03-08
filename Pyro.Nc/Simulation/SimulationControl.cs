using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;

namespace Pyro.Nc.Simulation;

public class SimulationControl : MachineComponent
{
    public MovementType Movement { get; set; }
    public UnitType Unit { get; set; }
    
    public void ResetSimulation()
    {
        var controller = Globals.Workpiece;
        controller.ResetVertices();
        controller.ResetColors();
        controller.ResetColors();
        var machine = MachineBase.CurrentMachine;
        machine.ChangeTool(0);
        machine.ToolControl.SelectedTool.Position = Globals.ReferencePointParser.BeginPoint;
        machine.StateControl.ResetControl();
        ResetSpindle();
        SoftResetCodeSimulation();
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