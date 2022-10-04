using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.Exceptions;
using Pyro.Nc.Parsing.GCommands;

namespace Pyro.Nc.Parsing.Rules
{
    public class CommandPriorityRule : Rule<List<ICommand>>
    {
        private static readonly Type[] _types = new Type[]
        {
            typeof(G04)
        };
        private static readonly Predicate<List<ICommand>> _predicate = list =>
        {
            var sc = list.FirstOrDefault(c => c.IsMatch(typeof(SpindleSpeedSetter)));
            if (sc is null)
            {
                return true;
            }
            for (var i = 0; i < list.Count; i++)
            {
                var command = list[i];
                var type = command.GetType();
                if (_types.Contains(type))
                {
                    command.Parameters.Values["S"] = sc.Parameters.GetValue("S");
                    list.Remove(sc);
                }
            }

            return true;
        };
        public CommandPriorityRule(string name)
            : base(name, _predicate, () => new RuleParseException("Undefined exception."))
        {
            
        }
    }
}