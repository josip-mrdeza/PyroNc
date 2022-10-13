using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Exceptions;

namespace Pyro.Nc.Parsing.Rules
{
    public class GroupRepetitionRule : Rule<List<ICommand>>
    {
        private static readonly int[] Counter = new int[5];
        public GroupRepetitionRule(string name)
            : base(name)
        {
        }

        public override bool CheckValidity(List<ICommand> list)
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
                    throw new RuleParseException($"A command of type {t.Family} (Last: {t.Description}) has been repeated multiple times in this line. This is forbidden.");
                }
            }

            return true;
        }
    }
}