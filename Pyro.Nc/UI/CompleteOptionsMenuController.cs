using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pyro.IO;
using Pyro.Nc.UI.Options;
using Pyro.Nc.UI.Options.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class CompleteOptionsMenuController : MonoBehaviour
{
    public TMP_Dropdown Dropdown;
    public TMP_InputField InputField;
    public Button Save;
    public LocalRoaming Roaming;
    public int SelectedPart;
    private void Start()
    {
        Roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Json");
        Assembly.GetExecutingAssembly().TraverseAssemblyAndCreateJsonFiles(Roaming);
        var src = JsonConfigCreator.Stores;
        Dropdown.ClearOptions();
        Dropdown.AddOptions(src.Select(x => x.Name).ToList());
        Dropdown.onValueChanged.AddListener(OnSelectionChanged);
        Save.onClick.AddListener(SaveData);
    }

    private void OnSelectionChanged(int arg0)
    {
        SelectedPart = arg0;
        var el = JsonConfigCreator.Stores[arg0];
        var prop = el.Parent.GetProperty(el.Name, BindingFlags.Static);
        InputField.text = prop.GetValue(null).ToString();
    }

    private void SaveData()
    {
        var el = JsonConfigCreator.Stores[SelectedPart];
        var prop = el.Parent.GetProperty(el.Name, BindingFlags.Static);
        prop.SetValue(null, float.Parse(InputField.text)); //for floats only
    }
}