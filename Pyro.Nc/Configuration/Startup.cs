using System;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Configuration.Updates;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class Startup : MonoBehaviour
    {
        private void Start()
        {
            PyroConsoleView.PushTextStatic("Application Startup initializing...");
            Collector.Init();
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