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
        public bool IsAsync { get; }
        public bool DisableAutoInit { get; }

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
                        new("No Tool", "emptyTool", 0, 0, 0, 0, 0, 0, 0, 0),
                        new("End mill 3mm", "end_mill_3mm", 1.5f, 1, 5, 0, Color.red),
                        new("End mill 6mm", "end_mill_6mm", 3f, 2, 12.3f, 0, Color.green),
                        new("End mill 12mm", "end_mill_12mm", 6f, 3, 10f, 0, Color.blue),
                        new("Face mill 40mm", "face_mill_40mm", 20f, 4, 12.212f, 0, Color.magenta),
                        new("Twist drill 8mm", "twist_drill_8mm", 4f, 5, 23, 0, Color.yellow)
                    };
                    foreach (var tool in Tools)
                    {
                        tool.RefreshToolLength();
                    }
                    Debug.Log($"Adding list to file {FileName}!");
                    Globals.Roaming.AddFile(FileName, Tools);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public Task InitAsync() => throw new NotImplementedException();
    }
}