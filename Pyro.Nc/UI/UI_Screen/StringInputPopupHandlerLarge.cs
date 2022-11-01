using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class StringInputPopupHandlerLarge : PopupHandler
    {
        private void Awake()
        {
            Globals.InputPopupHandlerLarge = this;
        }

        public override GameObject InitializeCrucial()
        {
            return Instantiate(Resources.Load("Popup_Prompt", typeof(GameObject)) as GameObject, Canvas.transform);
        }
    }
}