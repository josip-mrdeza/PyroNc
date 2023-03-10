using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public static class SynCommandHelper
{
    public static bool PopulateIfValid(string id, ref string[] parameterList, List<BaseCommand> commands)
    {
        var methods = typeof(SynCommandHelper).GetMethods(BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            bool b = (bool) method.Invoke(null, new object[]
            {
                id,
                parameterList,
                commands
            });
            if (b)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HandleTrigCondition(string id, string[] parameterList, List<BaseCommand> commands)
    {
        var tool = Globals.Tool;
        var parameters = new ArbitraryCommandParameters();
        TrigCommand command = id switch
        {
            "SinCommand" => new SinCommand(tool, parameters),
            "CosCommand" => new CosCommand(tool, parameters),
            "TanCommand" => new TanCommand(tool, parameters),
            _            => null
        };
        if (command == null)
        {
            return false;
        }
        commands.Add(command);
        return true;
    }
    public static bool HandleForLoopCondition(string id, string[] parameterList, List<BaseCommand> commands)
    {
        if (id.StartsWith("FOR"))
        {
            var reg = Regex.Matches(id, @"[\d]+");
            var reg2 = Regex.Match(id, @"[\w]+=");
            var r = reg.GetEnumerator();
            int i = 0;
            if (r.MoveNext())
            {
                int.TryParse(((Match)r.Current).Value, out i);
            }

            int iterations = 0;
            if (r.MoveNext())
            {
                int.TryParse(((Match)r.Current).Value, out iterations);
            }

            r.Reset();
            var command = new ForLoopGCode(i, iterations, reg2.Value.Replace("=", ""));
            commands.Add(command);
            var loop = command;
            if (CommandHelper.VariableMap.ContainsKey(loop.VariableName))
            {
                CommandHelper.VariableMap[loop.VariableName] = loop.StartIndex;
            }
            else
            {
                CommandHelper.VariableMap.Add(loop.VariableName, i);
            }

            return true;
        }

        if (id.StartsWith("ENDFOR"))
        {
            var command = new EndForGCode();
            commands.Add(command);
            return true;
        }

        return false;
    }
}