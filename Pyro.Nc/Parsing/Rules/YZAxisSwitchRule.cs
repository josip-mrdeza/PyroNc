using System.Collections;
using System.Collections.Generic;

namespace Pyro.Nc.Parsing.Rules
{
    public class YZAxisSwitchRule : Rule<List<ICommand>>
    {
        public YZAxisSwitchRule(string name) : base(name)
        {
        }

        public override void FixValidity(List<ICommand> value)
        {
            foreach (var command in value)
            {
                var p = command.Parameters;
                if (p.Values.ContainsKey("Y") && p.Values.ContainsKey("Z"))
                {
                    float y = p.Values["Y"];
                    float z = p.Values["Z"];

                    p.Values["Y"] = z;
                    p.Values["Z"] = y;
                }
            }
        }
    }
}