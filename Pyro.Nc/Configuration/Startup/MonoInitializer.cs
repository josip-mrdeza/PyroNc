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
                    PyroConsoleView.PushTextStatic($"Initialized '{root.GetType().Name}' in {individual.Elapsed.TotalMilliseconds.Round()} ms!");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            stopwatch.Stop();
            ViewHandler.Active = false;
            PyroConsoleView.PushTextStatic($"Completed 'MonoInitializer' in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
        }
    }
}