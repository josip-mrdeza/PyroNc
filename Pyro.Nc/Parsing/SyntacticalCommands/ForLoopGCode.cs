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
        if (ContainedCommands.Count == 0)
        {
            Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericMessage, "[ForLoop] - For loop contained no valid commands."));
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
    }
}