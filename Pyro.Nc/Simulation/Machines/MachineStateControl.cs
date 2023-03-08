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
    public bool IsResetting => State == MachineState.Resetting;
    
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
        Application.targetFrameRate = 240;
        QualitySettings.vSyncCount = 0;
    }

    public void FreeControl()
    {
        State = MachineState.Idle;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public void PauseControl()
    {
        State = MachineState.Paused;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public void ResetControl()
    {
        var runner = MachineBase.CurrentMachine.Runner;
        runner.Queue.Clear();
        ResetUI();
        State = MachineState.Idle;
        CommandHelper.PreviousModal = null;
        MachineBase.CurrentMachine.EventSystem.SystemReset();
        MachineBase.CurrentMachine.EventSystem.PEvents.Fire(Locals.EventConstants.SimulationReset);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public void ResetUI()
    {
        ViewHandler.ShowOne("3DView");
        UI_3D.Instance.SetTimeDisplay(new TimeSpan());
    }
}