using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.MCommands;

namespace Pyro.Nc.Parsing.Rules
{
    public class ToolCommandPriorityRule : Rule<List<ICommand>>
    {
        public ToolCommandPriorityRule(string name) : base(name)
        {
        }

        public override void FixValidity(List<ICommand> value)
        {
            var sc = value.FirstOrDefault(c => c.IsMatch(typeof(ToolSetter)));
            if (sc is null)
            {
                return;
            }
            for (var i = 0; i < value.Count; i++)
            {
                var command = value[i];
                var type = command.GetType();
                if (type == typeof(M06))
                {
                    command.Parameters.AddValue("value", sc.Parameters.GetValue("value"));
                    value.Remove(sc);
                }
            }
        }
    }
}