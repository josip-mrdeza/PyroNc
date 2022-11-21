using System;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;

namespace Pyro.Nc.Configuration.Managers
{
    public class InteropManager : IManager
    {
        public static object RichPresence;
        public bool IsAsync { get; }
        public bool DisableAutoInit { get; }

        public void Init()
        {
            var roaming = LocalRoaming.OpenOrCreate("PyroNc\\JGeneral");
            var fileId = "JGeneral.IO.Interop.dll";
            if (roaming.Exists(fileId))
            {
                PyroConsoleView.PushTextStatic($"Initializing discord rich presence:", $"ID = 962723957650382898");
                var assembly = Assembly.UnsafeLoadFrom(Globals.Roaming.Site + "JGeneral\\JGeneral.IO.Interop.dll");
                var type = assembly.GetType("JGeneral.IO.Interop.Discord.JRichPresence");
                var method = type.GetMethod("Create", new Type[]
                {
                    typeof(String)
                });
                RichPresence = method.Invoke(null, new object[]
                {
                    "962723957650382898"
                });
                PyroConsoleView.PushTextStatic($"Initialized Rich Presence.");
            }
            else
            {
                PyroConsoleView.PushTextStatic($"Failed to initialize rich presence, missing assembly components - '{fileId}'!");
            }
        }

        public Task InitAsync() => throw new NotImplementedException();
    }
}