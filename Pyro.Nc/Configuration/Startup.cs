using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class Startup : MonoBehaviour
    {
        public GameObject Tool;
        public Mesh DefaultMesh;
        public GameObject Cube;
        public GameObject PointerObject;
        private void Start()
        {
            PyroConsoleView.PushTextStatic("Application Startup initializing...");
            ManagerStorage.InitAll();
            var td = Tool.AddComponent<ToolDebug>();
            td.meshPointer = DefaultMesh;
            td.Plane = Cube;
            td.PObj = PointerObject;
            string appId = "PyNc";
            Globals.Info = SoftwareInfo.GetFromCache(appId);
            Globals.Info.Refresh(appId);
            // var fn2 = "dev.pyro";
            // if (roaming.Exists(fn2))
            // {
            //     Process.Start("PyroSoftwareUpdater.exe", "pack PyNc update.json");
            // }
            Collector.Init();
            PyroConsoleView.PushTextStatic("Collector Startup initialized!");
            PyroConsoleView.PushTextStatic("Starting periodic statistic thread...");
            Collector.SendStatisticsPeriodic();
            PyroConsoleView.PushTextStatic("Started periodic statistic thread!");
            PyroConsoleView.PushTextStatic("Statistics Startup initialized!");
            PyroConsoleView.PushTextStatic("Startup complete!");
        }
    }
}