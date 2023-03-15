using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pyro.Nc.Simulation.Machines;

namespace Pyro.Nc.Configuration;

public static class RegexPatterns
{
    public static Regex CompleteParameterCheck =
        new Regex(@"[xXyYzZiIjJdD]{1}?((\d+[,.]{1}\d+)|(\d+)|(\(.+\))|(\$\w+))", RegexOptions.Compiled);

    public static Regex CompleteGFunctionCheck =
        new Regex(@"[gG]{1}?\d{1,3}", RegexOptions.Compiled);

    public static Regex CompleteMFunctionCheck =
        new Regex(@"[mM]{1}?\d{1,3}", RegexOptions.Compiled);

    //public static Regex CompleteArbitraryFunctionCheck = CreateArbCmdRegex();

    private static Regex CreateArbCmdRegex()
    {
        var strings = GetAllArbitraryCommandNames();
        StringBuilder sb = new StringBuilder();
        foreach (var s in strings)
        {
            sb.Append('(');
            sb.Append(s);
            sb.Append(')');
            sb.Append('|');
        }

        //sb.Remove(sb.Length - 2, 1);

        return new Regex(sb.ToString(), RegexOptions.Compiled);
    }
    
    private static string[] GetAllArbitraryCommandNames()
    {
        var values = MachineBase.CurrentMachine.ToolControl.SelectedTool.Values;
        var vs = values.Storage;
        return vs.ArbitraryCommands.Keys.ToArray();
    }  
}