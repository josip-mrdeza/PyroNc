using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Programs
{
    public class Loader : View
    {
        public List<string> Files;
        public GameObject Template;
        public GameObject Port;
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
            var roaming = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
            for (var i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                var fb = Instantiate(Template, Port.transform);
                Objects.Add(fb);
                fb.name = file;
                fb.transform.position -= new Vector3(0, 50 * (i + 1), 0);
                fb.SetActive(true);
                var button = fb.GetComponent<Button>();
                button.GetComponentInChildren<TextMeshProUGUI>().text = file;
                button.onClick.AddListener(() =>
                {
                    InputHandler.Text.text = roaming.ReadFileAsText(file);
                });
            }
        }
        
    }
}