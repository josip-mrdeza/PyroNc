using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class RapidFeedCollisionException : Exception
    {
        public RapidFeedCollisionException(ICommand command) : base("Command '{0}': Collision with object!"
                                                                        .Format(command.GetType()))
        {
            
        }
    }
}