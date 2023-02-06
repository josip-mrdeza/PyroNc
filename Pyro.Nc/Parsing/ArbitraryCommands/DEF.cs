using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.ArbitraryCommands;

public class DEF : BaseCommand
{
    public DEF(VariableType definingType, string name, object value) : base(Globals.Tool, new ArbitraryCommandParameters())
    {
        DefiningType = definingType;
        Name = name;
        Value = value;
    }

    public VariableType DefiningType { get; }
    public string Name { get; }
    public object Value { get; }

    public override async Task Execute(bool draw)
    {
        if (CommandHelper.VariableMap.ContainsKey(Name))
        {
            CommandHelper.VariableMap[Name] = Value;
        }
        else
        {
            CommandHelper.VariableMap.Add(Name, Value);
        }
    }

    public static void ClearVariableMap()
    {
        CommandHelper.VariableMap.Clear();
    }
}