using Pyro.IO.Events;
using Pyro.Nc.Simulation.Workpiece;

namespace Pyro.Nc.Simulation;

public class SimBase
{
    public PyroEventSystem EventSystem { get; protected set; }
    public WorkpieceControl Workpiece { get; protected set; }
    public ReferencePointHandler References { get; protected set; }
}