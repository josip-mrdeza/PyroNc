using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pyro.Nc.Parsing;

public class GeneralParser
{
    public GeneralParser()
    {
        Passive = new Dictionary<string, BaseCommand>();
        GenerateTypes();
    }
    public Dictionary<string, BaseCommand> Passive { get; }
    public Dictionary<string, Type> Types { get; private set; }
    
    public IEnumerable<ParseResult> ParseFromLine(string line)
    {
        var blocks = line.Split(_separators);
        for (var i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            var type = FindByTypeName(block.ToUpperInvariant());
            if (type != null)
            {
                //find parameters
                var enumerable = YieldGCommandParameters(type, blocks, i);
                using var iterator = enumerable.GetEnumerator();
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }
            }
        }
    }

    public Type FindByTypeName(string block)
    {
        if (Types.TryGetValue(block, out var type))
        {
            return type;
        }

        return null;
    }

    public IEnumerable<ParseResult> FindParametersForGCommand(string[] blocks, int currIndex)
    {
        var currBlockEnumerator = blocks.GetEnumerator();
        for (int i = 0; i < currIndex; i++)
        {
            currBlockEnumerator.MoveNext();
        }
        while (currBlockEnumerator.MoveNext())
        {
            var currentBlock = currBlockEnumerator.Current as string;
            foreach (var parameter in _parameters)
            {
                if (currentBlock.StartsWith(parameter))
                {
                    yield return new ParseResult(currentBlock, true);
                }
                break;
            }
        }
    }
    
    private IEnumerable<ParseResult> YieldGCommandParameters(Type type, string[] blocks, int i)
    {
        if (type.Namespace == GNamespace)
        {
            using var enumerator = FindParametersForGCommand(blocks, i).GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
    public void ClearAllPassive()
    {
        Passive.Clear();
    }
    private void GenerateTypes()
    {
        Types = new Dictionary<string, Type>();
        var ass = Assembly.GetAssembly(typeof(GeneralParser));
        var types = ass.GetTypes();
        foreach (var type in types)
        {
            if (type.BaseType == typeof(ICommand))
            {
                Types.Add(type.Name.ToUpperInvariant(), type);
            }
        }
    }

    private readonly char[] _separators = new char[]
    {
        ' ','(',')'
    };

    private readonly string[] _parameters = new string[]
    {
        "X",
        "Y",
        "Z",
        "I",
        "J",
        "R",
        "CR"
    };

    private const string GNamespace = "Pyro.Nc.Parsing.GCommands";
    
    public struct ParseResult
    {
        public ParseResult(string textBlocks, bool isValid)
        {
            Text = textBlocks;
            IsValid = isValid;
        }
        public string Text { get; }
        public bool IsValid { get; }
    }
    //TODO
}