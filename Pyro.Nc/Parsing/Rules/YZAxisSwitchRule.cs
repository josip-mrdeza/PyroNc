using System.Collections;
using System.Collections.Generic;
using static System.Single;

namespace Pyro.Nc.Parsing.Rules
{
    public class YZAxisSwitchRule : Rule<List<BaseCommand>>
    {
        public YZAxisSwitchRule(string name) : base(name)
        {
        }

        public override void FixValidity(List<BaseCommand> value)
        {
            foreach (var command in value)
            {
                var p = command.Parameters;
                var containsY = p.Values.ContainsKey("Y");
                var containsZ = p.Values.ContainsKey("Z");
                if (containsY && containsZ)
                {
                    float y = p.Values["Y"];
                    float z = p.Values["Z"];

                    p.Values["Y"] = z;
                    p.Values["Z"] = y;
                }
                else if (!containsY && containsZ)
                {
                    p.Values["Y"] = p.Values["Z"];
                    p.Values["Z"] = NaN;
                }
                else if (containsY)
                {
                    p.Values["Z"] = p.Values["Y"];
                    p.Values["Y"] = NaN;
                }
            }
        }
    }
}