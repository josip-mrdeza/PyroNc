using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing.SyntacticalCommands;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class SingleButton : MonoInitializer
{
    public Button btn;
    public static bool Begun;
    public void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(async () =>
        {
            if (Begun)
            {
                if (MachineBase.CurrentMachine.Runner.Queue.Count > 0)
                {
                    if (MachineBase.CurrentMachine.Runner.CurrentContext is FORLOOP)
                    {
                        MachineBase.CurrentMachine.StateControl.FreeControl();
                        MachineBase.CurrentMachine.StateControl.LockFpsToExecutionMode();
                    }
                    else
                    {
                        MachineBase.CurrentMachine.StateControl.LockFpsToExecutionMode();
                        await MachineBase.CurrentMachine.Runner.ExecuteOne();
                    }
                }
                else
                {
                    Begun = false;
                }
            }
            else
            {
                MachineBase.CurrentMachine.StateControl.ResetUI();
                Globals.GCodeInputHandler.InsertGCodeIntoQueue(false);
                Begun = true;
                MachineBase.CurrentMachine.StateControl.FreeControl();
                MachineBase.CurrentMachine.StateControl.LockFpsToExecutionMode();
                await MachineBase.CurrentMachine.Runner.ExecuteOne();
            }
        });
    }
}