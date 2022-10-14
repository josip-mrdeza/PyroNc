using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.MCommands;

namespace Pyro.Nc.Parsing.Rules
{
    public class MCommandPriorityRule : Rule<List<ICommand>>
    {
        public MCommandPriorityRule(string name) 
            : base(name)
        {
        }

        public override bool CheckValidity(List<ICommand> value) => true;

        public override void FixValidity(List<ICommand> list)
        {
            if (list.Count < 2)
            {
                return;
            }

            if (list.FirstOrDefault(x => x.IsMatch(typeof(SpindleSpeedSetter))) != null && list.FirstOrDefault(x => x.IsMatch(typeof(M03))) != null)
            {
                list.Reverse();
            }
        }
    }                                                    
}