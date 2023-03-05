using System;
using System.Collections.Generic;
using System.Reflection;
using SharpMath.Expressions;

namespace Pyro.Nc.Parsing;

public class MathParser : SharpMath.Expressions.Parser
{
    public MathParser(string expression, Dictionary<string, object> variables) : base(expression)
    {
        var field = typeof(Token).GetField("_constantActions", BindingFlags.NonPublic | BindingFlags.Static);
        var instance = (Dictionary<string, Action<Stack<double>>>) field.GetValue(null);
        foreach (var addedKey in AddedKeys)
        {
            if (instance.ContainsKey(addedKey))
            {
                instance.Remove(addedKey);
            }
        }
        AddedKeys.Clear();
        foreach (var variable in variables)
        {
            if (!instance.ContainsKey(variable.Key))
            {
                instance[variable.Key] = stack =>
                {
                    stack.Push((double) variables[variable.Key]);
                };
            }
            else
            {
                instance.Add(variable.Key, stack =>
                {
                    stack.Push((double) variables[variable.Key]);
                });
                AddedKeys.Add(variable.Key);
            }
        }
    }

    public static readonly List<string> AddedKeys = new List<string>();
}