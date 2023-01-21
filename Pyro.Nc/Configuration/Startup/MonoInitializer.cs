using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
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
            await Logger.InitializeComplete();
            OnLoadedScript?.Invoke(Logger, TimeSpan.Zero, -1);
            Globals.Initializer = this;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Stopwatch individual = Stopwatch.StartNew();
            for (var i = 0; i < Scripts.Count; i++)
            {
                var root = Scripts[i];
                try
                {
                    //await Task.Delay(50);
                    individual.Restart();
                    await root.InitializeComplete();
                    individual.Stop();
                    PyroConsoleView.PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.MonoInitializerComplete,
                                                                             root.GetType().Name, 
                                                                             individual.Elapsed.TotalMilliseconds.Round().ToString()));
                    OnLoadedScript?.Invoke(root, individual.Elapsed, i);
                }
                catch (Exception e)
                {
                    Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericHandledError, e));
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