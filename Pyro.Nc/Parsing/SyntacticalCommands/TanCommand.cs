using System;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class TanCommand : TrigCommand
{
    public TanCommand(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
}