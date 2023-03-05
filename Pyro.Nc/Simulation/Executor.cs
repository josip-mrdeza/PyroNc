using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parser;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Simulation.Machines;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class Executor : MachineComponent
{
    public BaseCommand CurrentContext { get; private set; }

    public async Task ExecuteAll()
    {
        Machine.StateControl.ResetUI();
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
            if (command.Line > 0)
            {
                //Globals.GCodeInputHandler.SelectText(command.Line);
            }
            await command.ExecuteFinal(true, false);
            stateControl.FreeControl();
            CurrentContext = null;
        }
    }

    public async Task Jog(Vector3 position)
    {
        var bc = BaseCommand.Create<G01>();
        bc.Parameters = new GCommandParameters(position);
        Machine.ChangeTool(20);
        Machine.SetSpindleSpeed(500);
        Machine.SetFeedRate(200);
        Queue.Enqueue(bc);
        await ExecuteAll();
        Machine.SetSpindleSpeed(0);
        Machine.SetFeedRate(0);
        Machine.ChangeTool(0);
    }

    public Queue<BaseCommand> Queue { get; } = new Queue<BaseCommand>();
}