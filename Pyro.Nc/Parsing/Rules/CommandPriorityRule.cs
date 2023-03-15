using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;

namespace Pyro.Nc.Parsing.Rules
{
    public class CommandPriorityRule : Rule<List<BaseCommand>>
    {
        private static readonly Type[] Types = new Type[]
        {
            typeof(G04), typeof(M00)
        };
        
        public CommandPriorityRule(string name)
            : base(name)
        {
        }

        public override void FixValidity(List<BaseCommand> list)
        {
            var sc = list.FirstOrDefault(c => c.IsMatch(typeof(S)));
            if (sc is null)
            {
                return;
            }
            for (var i = 0; i < list.Count; i++)
            {
                var command = list[i];
                var type = command.GetType();
                if (Types.Contains(type))
                {
                    command.Parameters.Values["S"] = sc.Parameters.GetValue("S");
                    list.Remove(sc);
                }
            }
        }
    }
}