using System;

namespace Pyro.Nc.Parsing.Exceptions
{
    public class RapidFeedCollisionException : Exception
    {
        public RapidFeedCollisionException(ICommand command) : base($"Command '{command.GetType()}': Collision with object!")
        {
            
        }
    }
}