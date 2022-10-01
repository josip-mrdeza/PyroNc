using System;

namespace Pyro.Nc.Parsing.GCommands.Exceptions
{
    public class RapidFeedCollisionException : Exception
    {
        public RapidFeedCollisionException(ICommand command) : base($"Command '{command.GetType()}-{command.Description}': Exception message not implemented yet!")
        {
            
        }
    }
}