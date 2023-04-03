using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Simulation.Machines;

public class MachineStateControl : MachineComponent
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

    public void LockFpsToExecutionMode()
    {
        Application.targetFrameRate = 240;
        QualitySettings.vSyncCount = 0;
    }

    public void LockFpsToIdleMode()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public void BorrowControl()
    {
        State = MachineState.Executing;
        LockFpsToExecutionMode();
        Machine.Push("[MachineStateControl]: Borrowed control, FPS=240");
    }

    public void FreeControl()
    {
        State = MachineState.Idle;
        LockFpsToIdleMode();
        Machine.Push("[MachineStateControl]: Freed control, FPS=60");
    }

    public void PauseControl()
    {
        State = MachineState.Paused;
        LockFpsToIdleMode();
        Machine.Push("[MachineStateControl]: Paused control, FPS=60");
    }

    public void ResetControl()
    {
        var runner = MachineBase.CurrentMachine.Runner;
        runner.Queue.Clear();
        //ResetUI();
        State = MachineState.Resetting;
        CommandHelper.PreviousModal = null;
        MachineBase.CurrentMachine.EventSystem.SystemReset();
        MachineBase.CurrentMachine.EventSystem.PEvents.Fire(Locals.EventConstants.SimulationReset);
        LockFpsToIdleMode();
    }

    public void SetAwaitCompletion()
    {
        State = MachineState.WaitingForCompletion;
    }

    public void ResetUI()
    {
        ViewHandler.ShowOne("3DView");
        Sim2DView.Instance?.Clear();
        UI_3D.Instance.SetTimeDisplay(new TimeSpan());
        UI_3D.Instance.SetMessage("Reset");
        SingleButton.Begun = false;
    }
}