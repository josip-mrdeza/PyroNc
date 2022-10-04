using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI
{
    public class GCodeLoader : View
    {
        public List<string> Files;
        public Dropdown Dropdown;
        public GCodeInputHandler GCodeInput;
        public LocalRoaming Roaming;
        public override void Initialize()
        {
            Roaming = LocalRoaming.OpenOrCreate("PyroNc/GCode");
            Files = Roaming.Files.Values.Select(x => x.FullName).ToList();
            Dropdown.AddOptions(Roaming.Files.Keys.ToList());
            Dropdown.onValueChanged.AddListener(i =>
            {
                var text = File.ReadAllText(Files[i]);
                SetGText(text);
            });
            if (Files.Count > 0)
            {
                SetGText(File.ReadAllText(Files.First()));
            }
            base.Initialize();
        }

        public void SetGText(string text)
        {
            GCodeInput.Text.text = text;
        }
    }
}