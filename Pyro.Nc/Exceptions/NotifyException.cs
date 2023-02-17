using System;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.UI;
using Pyro.Nc.UI.UI_Screen;
using Pyro.Net;
using UnityEngine;

namespace Pyro.Nc.Exceptions
{
    public class NotifyException : Exception
    {
        public NotifyException(string message, bool asWarning = false) : base(message)
        {
            Globals.Comment.PushComment(base.GetBaseException().GetType().Name + ": " + message, asWarning ? Color.yellow : Color.red);
            if (asWarning)
            {
                return;
            }

            MachineBase.CurrentMachine.EventSystem.PEvents.Fire(Locals.EventConstants.ProgramEnd);
            PopupHandler.PopText(message);
            Task.Run(async () =>
            {
                await NetHelpers.Post("https://pyronetserver.azurewebsites.net/events/invoke?id=Exception&sequence=Pyro", $"Line {Globals.GCodeInputHandler.Line}-{message}");
            });
        }

        public NotifyException(string message, Exception ex, bool asWarning = false)
        {
            Contained = ex;
            Globals.Comment.PushComment(ex.GetType().Name + ": " + message, asWarning ? Color.yellow : Color.red);
            if (asWarning)
            {
                return;
            }
            MachineBase.CurrentMachine.EventSystem.PEvents.Fire(Locals.EventConstants.ProgramEnd);
            PopupHandler.PopText(message);
        }

        public Exception Contained { get; }

        public static TSystemException CreateNotifySystemException<TSystemException>(object sender, string message, bool asWarning = false) where TSystemException : Exception, new()
        {
            var exception = new TSystemException();
            var ex = new NotifyException(message, exception, asWarning);
            return exception;
        }
    }
}