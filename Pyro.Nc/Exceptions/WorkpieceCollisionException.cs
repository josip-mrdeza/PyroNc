using System;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Exceptions;

public class WorkpieceCollisionException : NotifyException
{
    public WorkpieceCollisionException(BaseCommand command) : base("~[{0}] - Collision with workpiece!~"
                                                                              .Format(command.ToString()))
    {
    }
}