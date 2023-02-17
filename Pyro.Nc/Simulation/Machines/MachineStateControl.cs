using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Simulation.Machines;

public class MachineStateControl
{
    public MachineState State { get; protected set; }

    public bool IsFree => State == MachineState.Idle;
    public bool IsPaused => State == MachineState.Paused;
    public bool IsExecuting => State == MachineState.Executing;
    
    public async Task WaitForControl()
    {
        if (State == MachineState.Resetting)
        {
            return;
        }
        while (State != MachineState.Idle)
        {
            await Task.Delay(Locals.IntConstants.ControlWaitDelay);
        }
    }

    public void BorrowControl()
    {
        State = MachineState.Executing;
    }

    public void FreeControl()
    {
        State = MachineState.Idle;
    }

    public void PauseControl()
    {
        State = MachineState.Paused;
    }

    public void ResetControl()
    {
        var runner = MachineBase.CurrentMachine.Runner;
        runner.Queue.Clear();
        ResetUI();
        State = MachineState.Idle;
        MachineBase.CurrentMachine.EventSystem.PEvents.Fire(Locals.EventConstants.SimulationReset);
    }

    private static void ResetUI()
    {
        ViewHandler.ShowOne("3DView");
        CommandHelper.PreviousModal = null;
        UI_3D.Instance.SetTimeDisplay(new TimeSpan());
    }
}