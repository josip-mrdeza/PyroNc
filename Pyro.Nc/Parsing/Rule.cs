using System;
using Pyro.Nc.Parsing.Exceptions;

namespace Pyro.Nc.Parsing
{
    public abstract class Rule
    {
        public string Name { get; }
        internal readonly Lazy<RuleParseException> Exception;
        private readonly Func<RuleParseException> _exceptionGetter;

        protected Rule(string name)
        {
            Name = name;
            _exceptionGetter ??= () => new RuleParseException(this);
            Exception = new Lazy<RuleParseException>(_exceptionGetter);
        }

        public virtual bool CheckValidity(object value) => true;

        public virtual void FixValidity(object value) {}
    }
    
    public abstract class Rule<T> : Rule
    {
        internal Rule(string name) : base(name)
        {
        }

        public abstract bool CheckValidity(T value);

        public virtual void FixValidity(T value) {}
    }
}