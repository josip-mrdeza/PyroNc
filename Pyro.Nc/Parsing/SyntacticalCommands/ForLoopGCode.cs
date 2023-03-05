using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class ForLoopGCode : BaseCommand
{
    public ForLoopGCode(int startIndex, int iterations, string variableName) : base(Globals.Tool, new ArbitraryCommandParameters())
    {
        VariableName = variableName;
        StartIndex = startIndex;  
        Iterations = iterations;
        ContainedCommands = new List<BaseCommand>();
    }

    public override string Description => $"({StartIndex.ToString()} -> {Iterations.ToString()}) - {(Iterations - StartIndex).ToString()} iterations";
    public string VariableName { get; }
    public int StartIndex { get; }
    public int CurrentIndex { get; private set; }
    public int Iterations { get; }
    public List<BaseCommand> ContainedCommands { get; }
    public override async Task Execute(bool draw)
    {
        SetVariableValue(StartIndex);
        
        if (ContainedCommands.Count == 0)
        {
            Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericMessage, "[ForLoop] - For loop contained no executable commands."));
            DeleteVariableValue();
            return;
        }

        for (CurrentIndex = StartIndex; CurrentIndex < Iterations; CurrentIndex++)
        {
            foreach (var command in ContainedCommands)
            {
                try
                {
                    await command.ExecuteFinal(draw);
                }
                catch (Exception e)
                {
                    Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, $"[ForLoop] - {e}"));
                }
            }
        }
        
        DeleteVariableValue();
        Globals.Console.Push("[ForLoop] - Deleted temporary index variable STARTINDEX!");
    }

    public void SetVariableValue(object value)
    {
        var varMap = CommandHelper.VariableMap;
        if (varMap.ContainsKey(VariableName))
        {
            varMap[VariableName] = value;
        }
        else
        {
            varMap.Add(VariableName, value);
        }
    }

    public void DeleteVariableValue()
    {
        var varMap = CommandHelper.VariableMap;
        varMap.Remove(VariableName);
    }
}