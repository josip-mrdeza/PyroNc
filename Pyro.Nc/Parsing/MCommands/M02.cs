using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.MCommands
{
    /// Change from Auto mode to MDI mode.Origin offsets are set to the default(like G54).
    /// Selected plane is set to XY plane(like G17).
    /// Distance mode is set to absolute mode(like G90).
    /// Feed rate mode is set to units per minute(like G94).Feed and speed overrides are set
    /// to ON(like M48).Cutter compensation is turned off (like G40).
    /// The spindle is stopped (like M5).
    ///The current motion mode is set to feed(like G1).Coolant is turned off (like M9).
    public class M02 : BaseCommand
    {
        public M02(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            var machine = MachineBase.CurrentMachine;
            machine.ChangeTool(0);
            machine.ToolControl.SelectedTool.Position = Globals.ReferencePointParser.BeginPoint;
            machine.StateControl.ResetControl();
            machine.SimControl.SoftResetCodeSimulation();
        }
    }
}