using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class CosCommand : TrigCommand
{
    public CosCommand(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
}