using System;
using System.Diagnostics;
using UnityEngine;

namespace Pyro.Nc.Configuration
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
    }
}