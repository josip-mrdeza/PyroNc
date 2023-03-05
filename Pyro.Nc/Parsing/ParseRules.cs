using System.Collections.Generic;
using UnityEngine;

namespace Pyro.Nc.Parsing
{
    public class ParseRules
    {
        public List<Rule<string>> TextRules { get; set; }
        public List<Rule<List<BaseCommand>>> CommandRules { get; set; }

        public BaseCommand PreviousModal
        {
            get => CommandHelper.PreviousModal;
            set => CommandHelper.PreviousModal = value;
        }

        public ParseRules()
        {
            TextRules = new List<Rule<string>>();
            CommandRules = new List<Rule<List<BaseCommand>>>();
        }

        public void AddRule(Rule<string> rule)
        {
            if (TextRules.Contains(rule))
            {
                return;
            }          
            TextRules.Add(rule);
        }
        
        public void AddRule(Rule<List<BaseCommand>> rule)
        {
            if (CommandRules.Contains(rule))
            {
                return;
            }          
            CommandRules.Add(rule);
        }

        public void TryAll(string text, List<BaseCommand> command)
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

        public void Try(List<BaseCommand> commands)
        {
            var lastModal = commands.FindLast(x => x.IsModal);
            if (lastModal != null)
            {
                PreviousModal = commands.FindLast(x => x.IsModal);
            }
            foreach (var commandRule in CommandRules)
            {
                var b = commandRule.CheckValidity(commands);
                ThrowIfInvalid(commandRule, b);
                commandRule.FixValidity(commands);
            }
        }
        
        public void Try(BaseCommand command)
        {
            var list = new List<BaseCommand>(1)
            {
                command
            };
            foreach (var commandRule in CommandRules)
            {
                var b = commandRule.CheckValidity(list);
                ThrowIfInvalid(commandRule, b);
                commandRule.FixValidity(list);
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