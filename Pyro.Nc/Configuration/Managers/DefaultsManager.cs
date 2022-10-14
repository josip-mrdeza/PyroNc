using System.Collections.Generic;
using Pyro.IO;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Managers
{
    public class DefaultsManager : IManager
    {
        public ToolValues Values;
        private const string DefaultsJson = "Defaults.json";
        public void Init()
        {
            Globals.DefaultsManager = this;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            if (!roaming.Exists(DefaultsJson))
            {
                Values = new ToolValues();
                roaming.AddFile(DefaultsJson, Values);
            }
            else
            {
                Values = roaming.ReadFileAs<ToolValues>(DefaultsJson);
            }
        }
    }
}