namespace Pyro.Nc.Parsing.Exceptions
{
    public class GroupRepetitionException : RuleParseException
    {
        public GroupRepetitionException(string groupName) : base($"Same '{groupName}' group programmed repeatedly.")
        {
        }
    }
}