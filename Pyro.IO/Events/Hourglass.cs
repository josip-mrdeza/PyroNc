using System;
using System.Diagnostics;

namespace Pyro.IO.Events;

public class Hourglass : IDisposable
{
    private static Hourglass FreeHourglassInstance;
    public string Name { get; private set; }
    public Stopwatch Stopwatch { get; private set; }
    public bool IsFree { get; private set; }
    public Action<Hourglass> Finishing { get; set; }

    public Hourglass(string name)
    {
        Name = name;
        Stopwatch = new Stopwatch();
        Begin();
        if (FreeHourglassInstance is null)
        {
            FreeHourglassInstance = this;
        }
    }

    private void Begin()
    {
        Stopwatch.Start();
        IsFree = false;
    }

    public void Dispose()
    {
        Stopwatch.Stop();
        // Globals.Console.Push(
        //     $"[Hourglass] - Method '{Name}' took {_stopwatch.Elapsed.TotalMilliseconds.Round(3)}ms to complete.");
        Finishing(this);
        Stopwatch.Reset();
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