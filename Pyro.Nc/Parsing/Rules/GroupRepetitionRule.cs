using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Exceptions;

namespace Pyro.Nc.Parsing.Rules
{
    public class GroupRepetitionRule : Rule<List<BaseCommand>>
    {
        private static readonly int[] Counter = new int[5];
        public GroupRepetitionRule(string name)
            : base(name)
        {
        }

        public override bool CheckValidity(List<BaseCommand> list)
        {
            for (int i = 0; i < Counter.Length; i++)
            {
                Counter[i] = 0;
            }
            for (var i = 0; i < list.Count; i++)
            {
                var t = list[i];
                var index = (int) t.Family;
                ref var ctr = ref Counter[index];
                if (t.Family != Group.Other)
                {
                    ctr += 1;
                }
                if (ctr > 1)
                {
                    // throw new RuleParseListException(
                    //     "A command of type {0} (Last: {1}) has been repeated multiple times in this line. This is forbidden."
                    //         .Format(t.Family, t.Description), list.Select(x => x.ToString()));
                }
            }

            return true;
        }
    }
}