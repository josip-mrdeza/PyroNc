using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Parsing.Rules;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class Startup : InitializerRoot
    {
        public static List<IManager> Managers;
        public override void Initialize()
        {
            PyroConsoleView.PushTextStatic("Application Startup initializing...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            InitializeManagers();
            var rules = Globals.Rules;
            rules.AddRule(new MCommandPriorityRule("SCPR"));
            rules.AddRule(new CommandPriorityRule("CPR"));
            rules.AddRule(new GroupRepetitionRule("GRR"));
            stopwatch.Stop();
            PyroConsoleView.PushTextStatic($"Startup complete in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
        }

        public void InitializeManagers()
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            if (!roaming.Exists("Managers.json"))
            {
                CreateManagersFromMemory(roaming);
            }
            else
            {
                CreateManagersFromDisk(roaming);
            }
        }

        private static void CreateManagersFromDisk(LocalRoaming roaming)
        {
            Stopwatch stopwatch = new Stopwatch();
            var fullNames = roaming.ReadFileAs<string[]>("Managers.json");
            Managers = new List<IManager>(fullNames.Length);
            foreach (var fullName in fullNames)
            {
                var manager = (IManager) Activator.CreateInstance(Type.GetType(fullName)!);
                stopwatch.Restart();
                manager.Init();  
                PyroConsoleView.PushTextStatic($"Manager '{fullName}' completed in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
                stopwatch.Stop();
                Managers.Add(manager);
            }
        }

        private static void CreateManagersFromMemory(LocalRoaming roaming)
        {
            Stopwatch stopwatch = new Stopwatch();
            Managers = new List<IManager>(10);
            var managersTypes = new List<string>();
            foreach (var type in typeof(ManagerStorage).Assembly.GetTypes())
            {
                if (type.GetInterface("IManager") is not null)
                {
                    var manager = (IManager) Activator.CreateInstance(type);
                    stopwatch.Restart();
                    manager.Init();  
                    PyroConsoleView.PushTextStatic($"Manager '{type.FullName}' completed in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
                    stopwatch.Stop();
                    Managers.Add(manager);
                    managersTypes.Add(type.FullName);
                }
            }

            roaming.AddFile("Managers.json", managersTypes);
        }
    }
}