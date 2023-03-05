using System;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands;

public class ValuePointerDereference : BaseCommand
{
    public ValuePointerDereference(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }

    public string Name { get; set; }

    public override string Description
    {
        get
        {
            var obj = GetValue();
            if (obj is null)
            {
                return "Failed to dereference the pointer.";
            }
            return $"*{Name} = {obj}";
        }
        internal set {}
    }
    
    public object Value => GetValue();

    public object GetValue()
    {
        var dict = CommandHelper.VariableMap;    
        if (dict.TryGetValue(Name, out var obj))
        {
            return obj;
        }

        return null;
    }
}