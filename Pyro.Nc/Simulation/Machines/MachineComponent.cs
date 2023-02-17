using Pyro.Nc.Configuration.Startup;

namespace Pyro.Nc.Simulation.Machines;

public abstract class MachineComponent
{
    public MachineBase Machine => MachineBase.CurrentMachine;

    public void Log(string s)
    {
        Globals.Comment.Push(s);
    }
}