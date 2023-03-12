using System;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;

namespace Pyro.Nc.Configuration;

public class PyroPluginPrimjer : IPyroPlugin
{
    public void InitializePlugin()
    {
        //Kod za inicijalizaciju
        Globals.Console.Push("PyroPlugin se dovrsio inicijalizirati.");
    }

    public void Update()
    {
        
    }
}