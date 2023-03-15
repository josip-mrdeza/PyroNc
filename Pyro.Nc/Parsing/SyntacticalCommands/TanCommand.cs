using System;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class TAN : TrigCommand
{
    public TAN(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
}