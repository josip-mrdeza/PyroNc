using System;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class AddNumerationsButton : MonoBehaviour
{
    public Button AddNumsButton;
    private void Start()
    {
        AddNumsButton = GetComponent<Button>();
        AddNumsButton.onClick.AddListener(() =>
        {
            PopupHandler.PopInputOption("Renumber blocks", "Renumber", p =>
            {
                var step = int.Parse(p.PrefabInputs[0].text);
                Globals.GCodeInputHandler.AddNumerations(step);
                Globals.Console.Push($"[AddNumerationsButton]: Added Ns with a step of '{step}'!");
            });
        });
    }
}