using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class SinCommand : TrigCommand
{
    public SinCommand(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
}