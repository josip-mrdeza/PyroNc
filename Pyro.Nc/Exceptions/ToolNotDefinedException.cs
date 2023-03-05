using Pyro.Nc.Simulation.Machines;

namespace Pyro.Nc.Exceptions
{
    public class ToolNotDefinedException : NotifyException
    {
        public ToolNotDefinedException()
            : base($"~[{MachineBase.CurrentMachine.Runner.CurrentContext}] - On line {MachineBase.CurrentMachine.Runner.CurrentContext.Line}; " +
                   $"Invalid operation, tool is not defined.~")
        {
            
        }
        
        public ToolNotDefinedException(string message) : base(message)
        {
        }
    }
}