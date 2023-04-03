using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.ArbitraryCommands;

public class DEF : BaseCommand
{
    public DEF(ToolBase tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
    {
    }
    public DEF(VariableType definingType, bool isArray, string name, object value, int n, int m) : base(Globals.Tool, new ArbitraryCommandParameters())
    {
        DefiningType = definingType;
        Name = name;
        Value = value;
        IsArrayType = isArray;
        N = n;
        M = m;
    }

    public VariableType DefiningType { get; }
    public string Name { get; }
    public object Value { get; }
    public bool IsArrayType { get; }
    public int N { get; }
    public int M { get; }

    public override string Description
    {
        get => $"{DefiningType.ToString().ToLower()} {Name} = {Value ?? "Null"}";
        internal set {}
    }

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
        foreach (var systemVariable in CommandHelper.SysStorage.Variables)
        {
            CommandHelper.VariableMap.Add(systemVariable.Id.ToString(), systemVariable.Value);
        }
    }

    public static object CreateVariableOfType(VariableType type, bool isArray, string value = "", int verticalLength = 0, int horizontalLength = 0)
    {
        switch (type)
        {
            case VariableType.INT:
            {
                if (isArray)
                {
                    return CreateVariableArray<int>(verticalLength, horizontalLength);
                }

                return int.Parse(value);
            }
            case VariableType.REAL:
            {
                if (isArray)
                {
                    return CreateVariableArray<double>(verticalLength, horizontalLength);
                }

                return double.Parse(value);
            }
            case VariableType.BOOL:
            {
                if (isArray)
                {
                    return CreateVariableArray<bool>(verticalLength, horizontalLength);
                }

                if (bool.TryParse(value, out var v))
                {
                    return v;
                }
                else
                {
                    if (int.TryParse(value, out var bv))
                    {
                        return bv != 0;
                    }

                    return false;
                }
            }
            case VariableType.CHAR:
            {
                if (isArray)
                {
                    return CreateVariableArray<char>(verticalLength, horizontalLength);
                }

                return char.Parse(value);
            }
            case VariableType.STRING:
            {
                if (isArray)
                {
                    return value;
                }
                return value;
            }
            case VariableType.AXIS:
            {
                return value;
            }
            case VariableType.FRAME:
            {
                return value;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    public static T[,] CreateVariableArray<T>(int n, int m)
    {
        T[,] arr = new T[n, m];

        return arr;
    }
}