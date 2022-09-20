using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public static class ManagerStorage
    {
        public static List<IManager> Managers = new List<IManager>();

        public static void InitAll()
        {
            var types = typeof(ManagerStorage).Assembly.GetTypes().Where(x => x.GetInterface("IManager") != null).ToArray();
            Managers = types.Select(x => (IManager) Activator.CreateInstance(x)).ToList();
            foreach (var manager in Managers)
            {
                manager.Init();
            }
        }
    }
}