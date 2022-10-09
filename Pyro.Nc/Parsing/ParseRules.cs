using System.Collections.Generic;
using UnityEngine;

namespace Pyro.Nc.Parsing
{
    public class ParseRules
    {
        public List<Rule<string>> TextRules { get; set; }
        public List<Rule<List<ICommand>>> CommandRules { get; set; }

        public ParseRules()
        {
            TextRules = new List<Rule<string>>();
            CommandRules = new List<Rule<List<ICommand>>>();
        }

        public void AddRule(Rule<string> rule)
        {
            if (TextRules.Contains(rule))
            {
                return;
            }          
            TextRules.Add(rule);
        }
        
        public void AddRule(Rule<List<ICommand>> rule)
        {
            if (CommandRules.Contains(rule))
            {
                return;
            }          
            CommandRules.Add(rule);
        }

        public void TryAll(string text, List<ICommand> command)
        {
            Try(text);
            Try(command);
        }
        
        public void Try(string text)
        {
            foreach (var textRule in TextRules)
            {
                var b = textRule.CheckValidity(text);
                ThrowIfInvalid(textRule, b);
                textRule.FixValidity(text);
            }
        }

        public void Try(List<ICommand> commands)
        {
            foreach (var commandRule in CommandRules)
            {
                var b = commandRule.CheckValidity(commands);
                ThrowIfInvalid(commandRule, b);
                commandRule.FixValidity(commands);
            }
        }

        public void ThrowIfInvalid(Rule rule, bool result)
        {
            if (!result)
            {
                Debug.LogWarning(rule.Exception.Value);
            }
        }
    }
}