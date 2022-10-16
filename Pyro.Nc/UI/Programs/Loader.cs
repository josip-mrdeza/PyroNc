using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using TrCore;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Programs
{
    public class Loader : View
    {
        public List<string> Files;
        public ListView Port;
        public GCodeInputHandler InputHandler;
        public override void Initialize()
        {
            base.Initialize();
            Load();
            ShowOnScreen();
        }

        public virtual void Load()
        {
            Files = LocalRoaming.OpenOrCreate("PyroNc\\GCode").ListAll().ToList();
        }

        public void ShowOnScreen()
        {
            //var roaming = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
            Port.Refresh(Files.Select(s => (object) s).ToList());
        }
        
    }
}