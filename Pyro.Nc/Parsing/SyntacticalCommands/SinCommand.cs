using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class SIN : TrigCommand
{
    public SIN(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
}