using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions
{
    public class RapidFeedCollisionException : WorkpieceCollisionException
    {
        public RapidFeedCollisionException(BaseCommand command) : base(command)
        {
            
        }
    }
}