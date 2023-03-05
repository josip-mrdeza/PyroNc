using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation.Machines;

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
        MCALL.ClearSubroutine();
        DEF.ClearVariableMap();
    }
}