using System;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using TMPro;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class EditButton : InitializerRoot
{
    public Button btn;
    public TextMeshProUGUI txt;

    public void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnEditClicked);
        txt = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnEditClicked()
    {
        var view = ViewHandler.Views[Globals.GCodeInputHandler.Id];
        if (view.IsActive)
        {
            ViewHandler.ShowOne("3DView");
            txt.text = "Edit";
        }
        else
        {
            MachineBase.CurrentMachine.SimControl.ResetSimulation();
            ViewHandler.ShowOne(Globals.GCodeInputHandler.Id);
            txt.text = "3D View";
        }
    }
}