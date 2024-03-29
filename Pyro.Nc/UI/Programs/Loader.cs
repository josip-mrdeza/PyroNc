using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Programs
{
    public class Loader : View
    {
        public List<string> Files;
        public ListView Port;
        public override void Initialize()
        {
            base.Initialize();
            Globals.Loader = this;
            Load();
            ShowOnScreen();
        }

        public override void Show()
        {
            base.Show();
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

        public override void UpdateView()
        {
            // if (Input.GetMouseButtonDown(3))
            // {
            //     var mousePos = (Vector2) Input.mousePosition;
            //     
            // }
        }
    }
}