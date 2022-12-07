using System;
using System.Diagnostics;
using Pyro.Math;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Statistics;

public class Hourglass : IDisposable
{
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private static Hourglass FreeHourglassInstance;
    public string Name { get; private set; }
    public bool IsFree { get; private set; }

    public Hourglass(string name)
    {
        Name = name;
        Begin();
        if (FreeHourglassInstance is null)
        {
            FreeHourglassInstance = this;
        }
    }

    private void Begin()
    {
        _stopwatch.Start();
        IsFree = false;
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        Globals.Console.Push(
            $"[Hourglass] - Method '{Name}' took {_stopwatch.Elapsed.TotalMilliseconds.Round(3)}ms to complete.");
        _stopwatch.Reset();
        IsFree = true;
    }

    public static Hourglass GetOrCreate(string name)
    {
        if (FreeHourglassInstance is null)
        {
            return new Hourglass(name);
        }

        if (FreeHourglassInstance.IsFree)
        {
            FreeHourglassInstance.Name = name;
            FreeHourglassInstance.Begin(); 
            return FreeHourglassInstance;
        }
        else
        {
            return new Hourglass(name);
        }
    }
}