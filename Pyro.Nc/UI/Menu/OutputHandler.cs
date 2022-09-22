using System;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI.Menu
{
    public class OutputHandler : MonoBehaviour
    {
        public TextMeshProUGUI ValueText;
        public TextMeshProUGUI DescriptionText;
        public string Description;

        private void Start()
        {
            DescriptionText.text = Description;
        }
    }
}