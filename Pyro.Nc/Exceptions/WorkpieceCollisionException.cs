using System;
using Pyro.Nc.Parsing;
using UnityEngine;

namespace Pyro.Nc.Exceptions;

public class WorkpieceCollisionException : NotifyException
{
    public WorkpieceCollisionException(BaseCommand command, Vector3 v) : base("~[{0}] - Collision with workpiece at {1}!~"
                                                                              .Format(command.ToString(), v.ToString()))
    {
    }
}