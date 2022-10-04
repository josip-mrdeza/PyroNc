using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.Parsing.Exceptions;

namespace Pyro.Nc.Parsing.Rules
{
    public class GroupRepetitionRule : Rule<List<ICommand>>
    {
        private static readonly int[] _counter = new int[5];
        private static readonly Predicate<List<ICommand>> _predicate = list =>
        {
            for (int i = 0; i < _counter.Length; i++)
            {
                _counter[i] = 0;
            }
            for (var i = 0; i < list.Count; i++)
            {
                var t = list[i];
                var index = (int) t.Family;
                ref var ctr = ref _counter[index];
                ctr += 1;
                if (ctr > 1)
                {
                    _exclusion = (Group) ctr;
                    return false;
                }
            }

            return true;
        };
        private static readonly Func<RuleParseException> _exGetter = () => new GroupRepetitionException(_exclusion.ToString());
        private static Group _exclusion;
        public GroupRepetitionRule(string name)
            : base(name, _predicate, _exGetter)
        {
        }
    }
}