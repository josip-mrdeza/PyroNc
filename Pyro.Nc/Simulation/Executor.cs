using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parser;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation.Machines;

namespace Pyro.Nc.Simulation;

public class Executor : MachineComponent
{
    public BaseCommand CurrentContext { get; private set; }

    public async Task ExecuteAll()
    {
        while (Queue.Count > 0)
        {
            if (Machine.StateControl.State == MachineState.Resetting)
            {
                break;
            }
            await ExecuteOne();
        }
    }
    public async Task ExecuteOne()
    {
        var stateControl = Machine.StateControl;
        await stateControl.WaitForControl();

        if (Queue.Count > 0)
        {
            var command = Queue.Dequeue();
            CurrentContext = command;
            stateControl.BorrowControl();
            await command.ExecuteFinal(true, false);
            stateControl.FreeControl();
            CurrentContext = null;
        }
    }

    public Queue<BaseCommand> Queue { get; } = new Queue<BaseCommand>();
}