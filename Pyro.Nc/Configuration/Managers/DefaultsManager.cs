using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Managers
{
    public class DefaultsManager : IManager
    {
        public ToolValues Values;
        private const string DefaultsJson = "Defaults.json";
        public bool IsAsync { get; }
        public bool DisableAutoInit { get; }

        public void Init()
        {
            Globals.DefaultsManager = this;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            if (!roaming.Exists(DefaultsJson))
            {
                Globals.Console.Push("Missing defaults.json, creating...");
                Values = new ToolValues();
                roaming.AddFile(DefaultsJson, Values);
                Globals.Console.Push("Created defaults.json!");
            }
            else
            {
                Values = roaming.ReadFileAs<ToolValues>(DefaultsJson);
            }
        }

        public Task InitAsync() => throw new System.NotImplementedException();
    }
}