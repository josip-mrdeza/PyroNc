using System;
using Pyro.Nc.Parsing;
using UnityEngine;

namespace Pyro.Nc.Exceptions
{
    public class RapidFeedCollisionException : WorkpieceCollisionException
    {
        public RapidFeedCollisionException(Vector3 v) : base($"~[{CurrentContext}]: Rapid feed collision exception \nat {v.ToString()}!~")
        {
            
        }
    }
}