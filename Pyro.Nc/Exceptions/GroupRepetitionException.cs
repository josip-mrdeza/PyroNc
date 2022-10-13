namespace Pyro.Nc.Exceptions
{
    public class GroupRepetitionException : RuleParseException
    {
        public GroupRepetitionException(string groupName) : base("Same '{0}' group programmed repeatedly in the same line, this is forbidden."
                                                                     .Format(groupName))
        {
        }
    }
}