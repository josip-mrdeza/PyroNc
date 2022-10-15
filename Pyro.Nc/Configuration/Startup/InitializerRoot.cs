using System;
using System.Diagnostics;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using TMPro;
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

        internal void Push(params string[] arr)
        {
            if (Globals.Console is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
            }
        }

        internal void PushComment(params string[] arr)
        {
            if (Globals.Console is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
            }

            if (Globals.Comment is not null)
            {
                Globals.Comment.Objects[1].GetComponent<TextMeshProUGUI>().text = string.Join(", ", arr);
            }
        }
    }
}