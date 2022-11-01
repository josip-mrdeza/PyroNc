using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class StringInputPopupHandlerSmall : PopupHandler
    {
        private void Awake()
        {
            Globals.InputPopupHandlerSmall = this;
        }

        public override GameObject InitializeCrucial()
        {
            return Instantiate(Resources.Load("Popup_Prompt_Small", typeof(GameObject)) as GameObject, Canvas.transform);
        }
    }
}