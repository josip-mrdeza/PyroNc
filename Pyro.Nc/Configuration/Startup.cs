using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class Startup : InitializerRoot
    {
        public static List<IManager> Managers = new List<IManager>();
        public override void Initialize()
        {
            PyroConsoleView.PushTextStatic("Application Startup initializing...");
            InitializeManagers();
            PyroConsoleView.PushTextStatic("Startup complete!");
        }

        public void InitializeManagers()
        {
            var types = typeof(ManagerStorage).Assembly.GetTypes().Where(x => x.GetInterface("IManager") != null).ToArray();
            Managers = types.Select(x => (IManager) Activator.CreateInstance(x)).ToList();
            foreach (var manager in Managers)
            {
                manager.Init();
            }
        }
    }
}