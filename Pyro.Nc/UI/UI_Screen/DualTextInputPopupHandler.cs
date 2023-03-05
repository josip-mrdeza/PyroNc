using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI.UI_Screen;

public class DualTextInputPopupHandler : PopupHandler
{
    public override GameObject InitializeCrucial()
    {
        Globals.DualTextInputPopupHandler = this;

        return Instantiate(Resources.Load<GameObject>("Popup_Prompt_DualInput"));
    }
}