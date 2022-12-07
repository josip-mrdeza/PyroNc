using System;
using System.Reflection;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Exceptions
{
    public class NotifyException : Exception
    {
        public NotifyException(string message) : base(message)
        {
            Globals.Comment.PushComment(base.GetBaseException().GetType().Name + ": " + message, Color.red);
            Globals.Tool.EventSystem.FireAsync("ProgramEnd").GetAwaiter().GetResult();
        }
        public static T Create<T>(object sender, string message) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), new object[]
            {
                message
            });
            ex.Source = sender.GetType().ToString();
            return ex as T;
        }
        
        public static T Create<T>(object sender) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), new object[] {default});
            ex.Source = sender.GetType().ToString(); 
            return ex as T;
        }
        
        public static T Create<T>(object sender, params object[] arr) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), arr);
            ex.Source = sender.GetType().ToString(); 
            return ex as T;
        }
    }
}