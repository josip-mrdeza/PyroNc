using System;
using System.Diagnostics;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration.Startup
{
    public abstract class InitializerRoot : MonoBehaviour
    {
        public bool IsInitialized;
        public abstract void Initialize();

        internal void InitializeComplete()
        {
            Initialize();
            var childrenCount = transform.childCount;
            if (childrenCount != 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = transform.GetChild(i);
                    var root = child.GetComponent<InitializerRoot>();
                    if (root is not null)
                    {
                        root.InitializeComplete();
                    }
                }
            }

            IsInitialized = true;
        }

        protected void Push(params string[] arr)
        {
            if (Globals.Console is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
            }
        }
    }
}