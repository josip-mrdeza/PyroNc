using System;
using System.Reflection;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Exceptions
{
    public class NotifyException : Exception
    {
        public NotifyException(string message, bool asWarning = false) : base(message)
        {
            Globals.Comment.PushComment(base.GetBaseException().GetType().Name + ": " + message, asWarning ? Color.yellow : Color.red);
            if (!asWarning)
            {
                return;
            }
            Globals.Tool.EventSystem.FireAsync("ProgramEnd").GetAwaiter().GetResult();
        }

        public NotifyException(string message, Exception ex, bool asWarning = false)
        {
            Contained = ex;
            Globals.Comment.PushComment(ex.GetType().Name + ": " + message, asWarning ? Color.yellow : Color.red);
            if (!asWarning)
            {
                return;
            }
            Globals.Tool.EventSystem.FireAsync("ProgramEnd").GetAwaiter().GetResult();
        }

        public Exception Contained { get; }

        public void Throw()
        {
            if (Contained != null)
            {
                throw Contained;
            }
            throw this;
        }
        
        public static T Create<T>(object sender, string message, bool asWarning = false) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), new object[]
            {
                message,
                asWarning
            });
            //ex.Source = sender.GetType().ToString();
            return ex as T;
        }
        
        public static T Create<T>(object sender, bool asWarning = false) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), new object[] {asWarning});
            //ex.Source = sender.GetType().ToString(); 
            return ex as T;
        }
        
        public static T Create<T>(object sender, params object[] arr) where T: NotifyException
        {
            var ex = (NotifyException) Activator.CreateInstance(typeof(T), arr);
            return ex as T;
        }

        public static TSystemException CreateNotifySystemException<TSystemException>(object sender, string message, bool asWarning = false) where TSystemException : Exception, new()
        {
            var exception = new TSystemException();
            var ex = new NotifyException(message, exception, asWarning);
            return exception;
        }
    }
}