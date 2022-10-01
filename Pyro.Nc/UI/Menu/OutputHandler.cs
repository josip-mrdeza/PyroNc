using System;
using Pyro.Nc.Configuration;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI.Menu
{
    public class OutputHandler : InitializerRoot
    {
        public TextMeshProUGUI ValueText;
        public TextMeshProUGUI DescriptionText;
        public string Description;

        public override void Initialize()
        {
            DescriptionText.text = Description;
        }
    }
}