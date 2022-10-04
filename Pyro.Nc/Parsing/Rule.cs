using System;
using Pyro.Nc.Parsing.Exceptions;

namespace Pyro.Nc.Parsing
{
    public class Rule
    {
        public string Name { get; }
        internal readonly Lazy<RuleParseException> Exception;
        private readonly Func<RuleParseException> _exceptionGetter;

        protected Rule(string name, Func<RuleParseException> exceptionGetter)
        {
            Name = name;
            _exceptionGetter ??= () => new RuleParseException(this);
            Exception = new Lazy<RuleParseException>(exceptionGetter ?? _exceptionGetter);
        }
    }
    
    public abstract class Rule<T> : Rule
    {
        internal readonly Predicate<T> Predicate;
        private readonly Predicate<T> _cachedPredicate;

        internal Rule(string name, Predicate<T> predicate, Func<RuleParseException> exceptionGetter = null) : base(name, exceptionGetter)
        {
            _cachedPredicate ??= _ => true;
            Predicate = predicate ?? _cachedPredicate;
        }
    }
}