using System;
using System.Collections.Generic;
using System.IO;
using Pyro.IO;

namespace Pyro.Nc.Configuration
{
    public class ProgramState : IManager
    {
        public static LocalVariables Variables = new LocalVariables();
        public static StreamWriter failedObjects;
        public static StreamWriter exceptions;
        public static List<Object> FailedObjects = new List<object>();
        public static List<Exception> Exceptions = new List<Exception>();
        public void Init()
        {
            Variables.Init("PyroNc");
            
            failedObjects = Variables.AddVariable("failedObjects.txt", FailedObjects).CreateText();
            exceptions = Variables.AddVariable("exceptions.txt", Exceptions).CreateText();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                FailedObjects.Add(sender);
                Exceptions.Add(args.ExceptionObject as Exception);
                failedObjects.WriteLine(sender ?? new object());
                exceptions.WriteLine(args.ExceptionObject as Exception);
            };
        }
    }
}