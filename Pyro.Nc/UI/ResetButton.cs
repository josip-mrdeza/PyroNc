using System;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ResetButton : InitializerRoot
{
    public static Button Instance;
    private Button _button;
    public override void Initialize()
    {
        _button = GetComponent<Button>();
        Instance = _button;
        _button.onClick.AddListener(MachineBase.CurrentMachine.SimControl.ResetSimulation);
    }
}