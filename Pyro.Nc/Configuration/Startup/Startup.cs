using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Injector;
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
            //gameObject.AddComponent<PDispatcher>();
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.SetQualityLevel(5);
            Push(Globals.Localisation.Find(Localisation.MapKey.StartupInitializeAsyncQuality, QualitySettings.GetQualityLevel().ToString()));
            Push(Globals.Localisation.Find(Localisation.MapKey.StartupAppInitializing));
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
            //Push(Globals.Localisation.Find(Localisation.MapKey.StartupComplete, stopwatch.Elapsed.TotalMilliseconds.Round().ToString()));
        }

        public async Task InitializeManagers()
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            await CreateManagersFromMemory(roaming);
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
                    try
                    {
                        if (manager.IsAsync)
                        {
                            await manager.InitAsync();
                        }
                        else
                        {
                            manager.Init();
                        }
                    }
                    catch (Exception e)
                    {
                        Push(Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, e.ToString()));
                    }
                    //Push(Globals.Localisation.Find(Localisation.MapKey.StartupCreateManagersFromMemoryCompleted, type.FullName, stopwatch.Elapsed.TotalMilliseconds.Round().ToString()));
                    stopwatch.Stop();
                    Managers.Add(manager);
                    managersTypes.Add(type.FullName);
                }
            }

            roaming.AddFile("Managers.json", managersTypes);
        }
    }
}