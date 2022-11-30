using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Managers;

public class LocalisationManager : IManager
{
    public bool IsAsync { get; }
    public bool DisableAutoInit => true;
    public void Init()
    {
        LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Localisations");
        if (!roaming.Exists("Localisation.txt"))
        {
            Localisation loc = new Localisation("EN", Localisation.EnglishMapping);
            roaming.AddFile("Localisation.txt", loc);
            Globals.Localisation = loc;
            return;
        }
        Localisation localisation = roaming.ReadFileAs<Localisation>("Localisation.txt");
        Globals.Localisation = localisation;
    }

    public Task InitAsync()
    {
        throw new NotImplementedException();
    }
}