namespace Pyro.Nc.Simulation.Machines;

public class MachineSpindleControl
{
    public Limiter FeedRate { get; } = new Limiter();
    public Limiter SpindleSpeed { get; } = new Limiter();
}