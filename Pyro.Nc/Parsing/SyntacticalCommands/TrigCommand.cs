using System;
using System.Collections.Generic;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public abstract class TrigCommand : BaseCommand
{
    protected TrigCommand(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }
    
    public float Value
    {
        get
        {
            var val = Parameters.GetValue("value");
            if (float.IsNaN(val))
            {
                throw new ParameterValueMismatchException(this, "value", true);
            }

            var type = this.GetType().Name;

            return type switch
            {
                "SinCommand" => val.Sin(),
                "CosCommand" => val.Cos(),
                "TanCommand" => val.Tan(),
                _ => throw new ParameterValueMismatchException(this, $"(Type:{type})", false)
            };
        }
    }
}