using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class ToolManager : IManager
    {
        public List<ToolConfiguration> Tools;
        public string FileName = "ToolConfig.json";
        public void Init()
        {
            try
            {
                if (Globals.Roaming.Exists(FileName))
                {
                    var json = Globals.Roaming.ReadFileAsText(FileName);
                    Tools = JsonSerializer.Deserialize<List<ToolConfiguration>>(json);
                }
                else
                {
                    Tools = new List<ToolConfiguration>()
                    {
                        new(3),
                        new(6),
                        new(8)
                    };
                    Debug.Log($"Adding list to file {FileName}!"); //asdka
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