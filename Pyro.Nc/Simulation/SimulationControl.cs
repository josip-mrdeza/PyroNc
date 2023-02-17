using Pyro.Nc.Configuration;
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
        var machine = MachineBase.CurrentMachine;
        machine.ChangeTool(0);
        machine.StateControl.ResetControl();
    }
}