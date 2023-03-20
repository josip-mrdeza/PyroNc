using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class FORLOOP : BaseCommand
{
    public FORLOOP(int startIndex, int iterations, string variableName) : base(Globals.Tool, new ArbitraryCommandParameters())
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
    public BaseCommand CurrentLoopContext { get; private set; }
    
    public override async Task Execute(bool draw)
    {
        SetVariableValue(StartIndex);
        
        if (ContainedCommands.Count == 0)
        {
            Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericMessage, "[ForLoop] - For loop contained no executable commands."));
            DeleteVariableValue();
            return;
        }

        if (SingleButton.Begun)
        {
            await IterateSingle(draw);
        }
        else
        {
            await IterateNormally(draw);
        }
        DeleteVariableValue();
        Globals.Console.Push("[ForLoop] - Deleted temporary index variable STARTINDEX!");
    }

    private async Task IterateSingle(bool draw)
    {
        for (CurrentIndex = StartIndex; CurrentIndex < Iterations; CurrentIndex++)
        {
            await IterateInternalSingle(draw);
        }
    }

    private async Task IterateNormally(bool draw)
    {
        for (CurrentIndex = StartIndex; CurrentIndex < Iterations; CurrentIndex++)
        {
            await IterateInternal(draw);
        }
    }
    
    private async Task IterateInternal(bool draw)
    {
        SetVariableValue(CurrentIndex);
        foreach (var command in ContainedCommands)
        {
            try
            {
                if (Machine.StateControl.IsResetting)
                {
                    CurrentLoopContext = null;
                    return;
                }
                Machine.StateControl.BorrowControl();
                CurrentLoopContext = command;
                UI_3D.Instance.SetMessage($"[ForLoop ({VariableName}={CurrentIndex})]: " + command.ToString());
                await command.ExecuteFinal(draw);
                if (Machine.StateControl.IsResetting)
                {
                    CurrentLoopContext = null;
                    return;
                }
                Machine.StateControl.FreeControl();
            }
            catch (Exception e)
            {
                Globals.Console.Push(
                    Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, $"~[ForLoop ({VariableName}={CurrentIndex})]: {e}!~"));

                throw;
            }
        }
    }

    private async Task IterateInternalSingle(bool draw)
    {
        SetVariableValue(CurrentIndex);
        foreach (var command in ContainedCommands)
        {
            try
            {
                CurrentLoopContext = command;
                UI_3D.Instance.SetMessage($"[ForLoop ({VariableName}={CurrentIndex})]: " + command.ToString());
                await command.ExecuteFinal(draw);
            }
            catch (Exception e)
            {
                Globals.Console.Push(
                    Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, $"[ForLoop ({VariableName}={CurrentIndex})]: {e}"));

                throw;
            }
            if (Machine.StateControl.IsResetting)
            {
                CurrentLoopContext = null;
                return;
            }
            Machine.StateControl.BorrowControl();
            await Machine.StateControl.WaitForControl();
        } 
    }

    public void SetVariableValue(object value)
    {
        var varMap = CommandHelper.VariableMap;
        if (varMap.ContainsKey(VariableName))
        {
            varMap[VariableName] = value;
            Globals.Console.Push($"Updated variable '{VariableName}' with value: {value}");
        }
        else
        {
            varMap.Add(VariableName, value);
            Globals.Console.Push($"Added variable '{VariableName}' with value: {value}");
        }
    }

    public void DeleteVariableValue()
    {
        var varMap = CommandHelper.VariableMap;
        varMap.Remove(VariableName);
    }
}