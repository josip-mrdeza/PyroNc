using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using Pyro.Nc.UI.Programs;
using Pyro.Nc.UI.UI_Screen;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation
{
    public static class Globals
    {
        private static readonly Func<object, bool> _isNetworkPresent = o =>
            Application.internetReachability != NetworkReachability.NotReachable;
        public static ITool Tool;
        public static WorkpieceController Workpiece;
        public static ToolManager ToolManager;
        public static PyroConsoleView Console;
        public static CommentView Comment;
        public static LocalRoaming Roaming;
        public static SoftwareInfo Info;
        public static MethodStateManager MethodManager;
        public static DefaultsManager DefaultsManager;
        public static MonoInitializer Initializer;
        public static PopupHandler DoublePopupHandler;
        public static PopupHandler InputPopupHandlerLarge;
        public static PopupHandler InputPopupHandlerSmall;
        public static PopupHandler TextPopupHandler;
        public static ReferencePointHandler ReferencePointHandler;
        public static ReferencePointParser ReferencePointParser;
        public static ParseRules Rules = new ParseRules();
        public static Loader Loader;
        public static Localisation Localisation;

        public static bool IsNetworkPresent = Pyro.Threading.PyroDispatcher.ExecuteOnMain(_isNetworkPresent, null);
    }
}