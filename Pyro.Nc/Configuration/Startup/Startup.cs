using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing.Rules;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;
using UnityEngine.Device;

namespace Pyro.Nc.Configuration.Startup
{
    public class Startup : InitializerRoot
    {
        public static List<IManager> Managers;
        public override async Task InitializeAsync()
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.SetQualityLevel(5);
            Push($"QualityLevel: {QualitySettings.GetQualityLevel().ToString()}");
            Push("Application Startup initializing...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            await InitializeManagers();
            var rules = Globals.Rules;
            rules.AddRule(new MCommandPriorityRule("SCPR"));
            rules.AddRule(new CommandPriorityRule("CPR"));
            rules.AddRule(new ToolCommandPriorityRule("TCPR"));
            rules.AddRule(new GroupRepetitionRule("GRR"));
            rules.AddRule(new YZAxisSwitchRule("YZASS")); //lmao
            rules.AddRule(new UnknownParameterRule("UPR"));
            stopwatch.Stop();
            Push($"Startup complete in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
        }

        public async Task InitializeManagers()
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            // if (!roaming.Exists("Managers.json"))
            // {
            //     CreateManagersFromMemory(roaming);
            // }
            // else
            // {
            //     CreateManagersFromDisk(roaming);
            // }
            await CreateManagersFromMemory(roaming);
        }

        private void CreateManagersFromDisk(LocalRoaming roaming)
        {
            Stopwatch stopwatch = new Stopwatch();
            var fullNames = roaming.ReadFileAs<string[]>("Managers.json");
            Managers = new List<IManager>(fullNames.Length);
            foreach (var fullName in fullNames)
            {
                var manager = (IManager) Activator.CreateInstance(Type.GetType(fullName)!);
                if (manager.DisableAutoInit)
                {
                    continue;
                }
                stopwatch.Restart();
                manager.Init();  
                Push($"Manager '{fullName}' completed in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
                stopwatch.Stop();
                Managers.Add(manager);
            }
        }

        private async Task CreateManagersFromMemory(LocalRoaming roaming)
        {
            Stopwatch stopwatch = new Stopwatch();
            Managers = new List<IManager>(15);
            var managersTypes = new List<string>();
            foreach (var type in typeof(ManagerStorage).Assembly.GetTypes())
            {
                if (type.GetInterface("IManager") is not null)
                {
                    var manager = (IManager) Activator.CreateInstance(type);
                    if (manager.DisableAutoInit)
                    {
                        continue;
                    }
                    stopwatch.Restart();
                    if (manager.IsAsync)
                    {
                        await manager.InitAsync();
                    }
                    else
                    {
                        manager.Init();
                    }
                    Push($"Manager '{type.FullName}' completed in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
                    stopwatch.Stop();
                    Managers.Add(manager);
                    managersTypes.Add(type.FullName);
                }
            }

            roaming.AddFile("Managers.json", managersTypes);
        }
    }
}