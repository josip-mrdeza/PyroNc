using System;
using System.Collections.Generic;
using Pyro.IO;
using PyroLauncher.Api;

namespace Pyro.Nc.Configuration
{
    public class LauncherDataManager : IManager
    {
        public AppConfiguration PyNcConfiguration;
        private string AppsFileID = "Apps.json";

        public void Init()
        {
            const string pyronc = "PyroNc";
            PyNcConfiguration = new AppConfiguration(pyronc, "no desc", $"{Environment.CurrentDirectory}\\PyNc.exe", null);
            var launcherRoaming = LocalRoaming.OpenOrCreate("Pyro Launcher");
            List<AppConfiguration> appConfigs = null;
            if (!launcherRoaming.Exists(AppsFileID))
            {
                launcherRoaming.AddFile(AppsFileID, new List<AppConfiguration>()
                {
                    PyNcConfiguration
                });

                return;
            }
            appConfigs = launcherRoaming.ReadFileAs<List<AppConfiguration>>(AppsFileID);
            if (!appConfigs.Exists(x => x.Name == pyronc))
            {
                appConfigs.Add(PyNcConfiguration);
            }
            
            launcherRoaming.ModifyFile(AppsFileID, appConfigs);
        }
    }
}