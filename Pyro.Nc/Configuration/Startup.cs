using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class Startup : MonoBehaviour
    {
        public static object RichPresence;
        private void Start()
        {
            var assembly = Assembly.UnsafeLoadFrom(Globals.Roaming.Site + "JGeneral\\JGeneral.IO.Interop.dll");
            var type = assembly.GetType("JGeneral.IO.Interop.Discord.JRichPresence");
            var method = type.GetMethod("Create", new Type[]{typeof(String)});
            RichPresence = method.Invoke(null, new object[]
            {
                "962723957650382898"
            });
            PyroConsoleView.PushTextStatic("Application Startup initializing...");
            Collector.Init();
            PyroConsoleView.PushTextStatic($"Initializing discord rich presence:",$"ID = 962723957650382898");
            PyroConsoleView.PushTextStatic($"Initialized Rich Presence.");
            PyroConsoleView.PushTextStatic("Collector Startup initialized!");
            //var version = UpdateInfo.GetLatest().GetAwaiter().GetResult();
            PyroConsoleView.PushTextStatic("Starting periodic statistic thread...");
            Collector.SendStatisticsPeriodic();
            PyroConsoleView.PushTextStatic("Started periodic statistic thread!");
            PyroConsoleView.PushTextStatic("Statistics Startup initialized!");
            PyroConsoleView.PushTextStatic("Startup complete!");
        }

        private void Update()
        {
            Globals.IsNetworkPresent = Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}