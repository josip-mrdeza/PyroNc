using System;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI.UI_Screen;

public class DualTextInputPopupHandler : PopupHandler
{
    public override void Awake()
    {
        Globals.DualTextInputPopupHandler = this;
        Initialize();
    }

    public override GameObject InitializeCrucial()
    {
        return Instantiate(Resources.Load<GameObject>("Popup_Prompt_DualInput"));
    }
}