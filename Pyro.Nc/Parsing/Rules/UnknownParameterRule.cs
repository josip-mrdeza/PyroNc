namespace Pyro.Nc.Parsing.Rules
{
    public class UnknownParameterRule : Rule<string>
    {
        public UnknownParameterRule(string name) : base(name)
        {
        }

        public override void FixValidity(string value)
        {
        }
    }
}