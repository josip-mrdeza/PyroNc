using System;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Exceptions
{
    public class NotifyException : Exception
    {
        public NotifyException(string message) : base(message)
        {
            Globals.Comment.PushComment(base.GetBaseException().GetType().Name + ": " + message, Color.red);
        }
    }
}