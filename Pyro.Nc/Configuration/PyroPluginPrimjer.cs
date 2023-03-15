using System;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;

namespace Pyro.Nc.Configuration;

public class PyroPluginPrimjer_2 : IPyroPlugin
{
    public PyroPluginPrimjer_2()
    {
        //Prazan ili defaultni konsturktor (constructor).
    }
    /// <summary>
    /// Funkcija za inicijalizaciju
    /// </summary>
    public void InitializePlugin()
    {
        //Kod za inicijalizaciju
        MachineBase.CurrentMachine.EventSystem.OnPositionChanged += (_, pozicijaAlata) =>
        {
            Globals.Console.Push($"Alat se pomaknuo, sada je na poziciji: {pozicijaAlata.ToString()}");
        };
    }
    /// <summary>
    /// Funkcija za periodicno ponavljanje koda
    /// </summary>
    public void Update()
    {
        //Ova funkcija izvrsavati ce se svaki frame
    }
}