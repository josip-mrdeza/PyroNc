using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;

namespace Pyro.Nc.Parsing.Rules
{
    public class CommandPriorityRule : Rule<List<ICommand>>
    {
        private static readonly Type[] Types = new Type[]
        {
            typeof(G04), typeof(M00)
        };
        
        public CommandPriorityRule(string name)
            : base(name)
        {
        }

        public override bool CheckValidity(List<ICommand> list)
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
                if (Types.Contains(type))
                {
                    command.Parameters.Values["S"] = sc.Parameters.GetValue("S");
                    list.Remove(sc);
                }
            }

            return true;
        }
    }
}