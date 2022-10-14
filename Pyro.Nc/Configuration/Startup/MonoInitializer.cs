using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pyro.Math;
using Pyro.Nc.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Configuration.Startup
{
    public class MonoInitializer : MonoBehaviour
    {
        public PyroConsoleView Logger;
        public List<InitializerRoot> Scripts;

        private void Start()
        {
            Logger.InitializeComplete();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Stopwatch individual = Stopwatch.StartNew();
            for (var i = 0; i < Scripts.Count; i++)
            {
                var root = Scripts[i];
                try
                {
                    individual.Restart();
                    root.InitializeComplete();
                    individual.Stop();
                    PyroConsoleView.PushTextStatic($"Initialized '{root.name}' in {individual.Elapsed.TotalMilliseconds.Round()} ms!");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            stopwatch.Stop();
            PyroConsoleView.PushTextStatic($"Completed 'MonoInitializer' Startup in {stopwatch.Elapsed.TotalMilliseconds.Round()} ms!");
        }
    }
}