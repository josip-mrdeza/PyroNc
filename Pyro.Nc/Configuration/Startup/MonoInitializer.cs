using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    PyroConsoleView.PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.MonoInitializerComplete,
                                                                             root.GetType().Name, 
                                                                             individual.Elapsed.TotalMilliseconds.Round().ToString()));
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
        }
    }
}