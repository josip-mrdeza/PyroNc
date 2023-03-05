using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.Configuration.Startup
{
    public abstract class InitializerRoot : MonoBehaviour
    {
        protected bool IsInitialized;
        public virtual void Initialize()
        {
            
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeComplete()
        {
            Initialize();
            await InitializeAsync();
            var childrenCount = transform.childCount;
            if (childrenCount != 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = transform.GetChild(i);
                    var root = child.GetComponent<InitializerRoot>();
                    if (root is not null)
                    {
                        await root.InitializeComplete();
                    }
                }
            }

            IsInitialized = true;
        }

        public void Push(params string[] arr)
        {
            #if DEBUG
            if (Globals.Console is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
            }
            #endif
        }

        public void PushComment(string arr, Color color)
        {
            #if DEBUG
            if (Globals.Console is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
            }
            else if (Globals.Comment is not null)
            {
                PyroConsoleView.PushTextStatic(arr);
                var textMeshProUGUI = Globals.Comment.Objects[1].GetComponent<TextMeshProUGUI>();
                textMeshProUGUI.text = arr;
                textMeshProUGUI.color = color;
            }
            #endif
        }
    }
}