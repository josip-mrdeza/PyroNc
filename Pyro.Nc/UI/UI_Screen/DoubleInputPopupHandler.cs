using System;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class DoubleInputPopupHandler : PopupHandler
    {
        private void Awake()
        {
            Globals.DoublePopupHandler = this;
        }

        public override GameObject InitializeCrucial()
        {
            return Instantiate(Resources.Load("Popup", typeof(GameObject)) as GameObject, Canvas.transform);
        }
    }
}