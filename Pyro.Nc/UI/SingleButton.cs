using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class SingleButton : MonoInitializer
{
    public Button btn;
    public bool Begun;
    public void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(async () =>
        {
            if (Begun)
            {
                if (MachineBase.CurrentMachine.Runner.Queue.Count > 0)
                {
                    await MachineBase.CurrentMachine.Runner.ExecuteOne();
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
                await MachineBase.CurrentMachine.Runner.ExecuteOne();
            }
        });
    }
}