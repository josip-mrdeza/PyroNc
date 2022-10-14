using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Configuration.Managers
{
    public class ToolManager : IManager
    {
        public List<ToolConfiguration> Tools;
        public string FileName = "ToolConfig.json";
        public void Init()
        {
            Globals.ToolManager = this;
            try
            {
                if (Globals.Roaming.Exists(FileName))
                {
                    var json = Globals.Roaming.ReadFileAsText(FileName);
                    Tools = JsonSerializer.Deserialize<List<ToolConfiguration>>(json, new JsonSerializerOptions()
                    {
                        IncludeFields = false,
                        WriteIndented = true
                    });
                }
                else
                {
                    Tools = new List<ToolConfiguration>()
                    {
                        new("end_mill_3mm", 1.5f, 1, Color.red),
                        new("end_mill_6mm", 3f, 2, Color.green),
                        new("end_mill_12mm", 6f, 3, Color.blue),
                        new("face_mill_40mm", 20f, 4, Color.magenta),
                        new("twist_drill_8mm", 4f, 5, Color.yellow)
                    };
                    Debug.Log($"Adding list to file {FileName}!");
                    Globals.Roaming.AddFile(FileName, Tools);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}