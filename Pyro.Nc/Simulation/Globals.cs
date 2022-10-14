using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation
{
    public static class Globals
    {
        private static readonly Func<object, bool> _isNetworkPresent = o =>
            Application.internetReachability != NetworkReachability.NotReachable;
        public static ITool Tool;
        public static ToolManager ToolManager;
        public static PyroConsoleView Console;
        public static CommentView Comment;
        public static LocalRoaming Roaming;
        public static SoftwareInfo Info;
        public static MethodStateManager MethodManager;
        public static DefaultsManager DefaultsManager;
        public static ParseRules Rules = new ParseRules();

        public static bool IsNetworkPresent = Pyro.Threading.PyroDispatcher.ExecuteOnMain(_isNetworkPresent, null);
    }
}