using System;
using System.Collections.Generic;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class MonoInitializer : MonoBehaviour
    {
        public PyroConsoleView Logger;
        public List<InitializerRoot> Scripts;

        private void Start()
        {
            Logger.InitializeComplete();
            for (var i = 0; i < Scripts.Count; i++)
            {
                var root = Scripts[i];
                try
                {
                    root.InitializeComplete();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}