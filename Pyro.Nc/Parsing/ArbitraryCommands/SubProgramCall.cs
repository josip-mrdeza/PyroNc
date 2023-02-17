using System;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.UI;

namespace Pyro.Nc.Parsing.ArbitraryCommands;

public class SubProgramCall : BaseCommand                                        
{
    public SubProgramCall(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
    {
    }

    public override string Description => Name;
    
    public string Name { get; set; }

    public override async Task Execute(bool draw)
    {
        Globals.Console.Push($"Sub program call: {Description}!");
        var fn = Description;
        if (!fn.EndsWith(".spf", StringComparison.InvariantCultureIgnoreCase))
        {
            fn += ".spf";
        }
        var text = LocalRoaming.OpenOrCreate("PyroNc\\GCode").ReadFileAsText(fn);
        await Globals.GCodeInputHandler.Call(text, false);
    }
}