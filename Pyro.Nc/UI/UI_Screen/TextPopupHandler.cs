using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI.UI_Screen
{
    public class TextPopupHandler : PopupHandler
    {
        private void Awake()
        {
            Globals.TextPopupHandler = this;
        }

        public override GameObject InitializeCrucial()
        {
            return Instantiate(Resources.Load("Popup_Text", typeof(GameObject)) as GameObject, Canvas.transform);
        }
    }
}