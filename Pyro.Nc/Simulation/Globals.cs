using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Simulation
{
    public static class Globals
    {
        public static ITool Tool;
        public static PyroConsoleView Console;
        public static CommentView Comment;
        public static LocalVariables Variables;
        public static bool IsNetworkPresent = true;
    }
}