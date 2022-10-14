using System.Collections.Generic;
using Pyro.IO;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Managers
{
    public class MethodStateManager : IManager
    {
        private Dictionary<string, MethodState> MethodStates;
        private const string MethodStatesJson = "MethodStates.json";
        public void Init()
        {
            Globals.MethodManager = this;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            if (roaming.Exists(MethodStatesJson))
            {
                MethodStates = roaming.ReadFileAs<Dictionary<string, MethodState>>(MethodStatesJson);
            }
            else
            {
                MethodStates = new Dictionary<string, MethodState>();
                roaming.AddFile(MethodStatesJson, MethodStates);
            }
        }

        public MethodState Get(string id)
        {
            if (MethodStates.ContainsKey(id))
            {
                return MethodStates[id];
            }

            throw new MethodStateMissingException(id);
        }
    }
}