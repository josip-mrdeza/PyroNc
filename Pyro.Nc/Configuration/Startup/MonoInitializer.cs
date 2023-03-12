using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using TinyClient;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Configuration.Startup
{
    public class MonoInitializer : MonoBehaviour
    {
        public PyroConsoleView Logger;
        public List<InitializerRoot> Scripts;
        private async void Start()
        {
            try
            {
                await Logger.InitializeComplete();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            var lr = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Json");
            Assembly.GetAssembly(typeof(MonoInitializer)).TraverseAssemblyAndCreateJsonFiles(lr);
            JsonConfigCreator.AssignJsonStoresToStaticInstances(JsonConfigCreator.Stores, lr, (parent, field, value) =>
            {
                Globals.Console.Push($"[{parent}] - Loaded config for property '{field}' with a value of '{value}'!");
            }, s => Globals.Console.Push(s));
            
            OnLoadedScript?.Invoke(Logger, TimeSpan.Zero, -1);
            Globals.Initializer = this;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Stopwatch individual = Stopwatch.StartNew();
            for (var i = 0; i < Scripts.Count; i++)
            {
                var root = Scripts[i];
                try
                {
                    individual.Restart();
                    await root.InitializeComplete();
                    individual.Stop();
                    OnLoadedScript?.Invoke(root, individual.Elapsed, i);
                }
                catch (Exception e)
                {
                    Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, root.GetType().Name, e.ToString()));
                }
            }
            stopwatch.Stop();
            ViewHandler.Active = false;
            PyroConsoleView.PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.MonoInitializerCompleteFull, 
                                                                     stopwatch.Elapsed.TotalMilliseconds.Round().ToString()));
            OnCompletedInitialization?.Invoke(this, stopwatch.Elapsed);
        }

        public event Action<InitializerRoot, TimeSpan, int> OnLoadedScript;
        public event Action<MonoInitializer, TimeSpan> OnCompletedInitialization;
    }
}