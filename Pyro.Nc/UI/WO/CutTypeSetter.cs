using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using TMPro;

namespace Pyro.Nc.UI.WO;

public class CutTypeSetter : InitializerRoot
{
    public TMP_Dropdown Dropdown;
    internal static LocalRoaming lr = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Json");
    public override void Initialize()
    {
        Dropdown = GetComponentInChildren<TMP_Dropdown>();
        Dropdown.ClearOptions();
        var type = typeof(CutType);
        var enums = type.GetEnumNames();
        Dropdown.AddOptions(enums.ToList());
        Dropdown.value = (int) Sim3D.CuttingType;
        Dropdown.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(int index)
    {
        Sim3D.CuttingType = (CutType)index;
        var arr = lr.ReadFileAs<List<StoreJsonPart>>("Sim3D.json");
        var sjp = arr.Find(x => x.Name == nameof(Sim3D.CuttingType));
        sjp.Value = Sim3D.CuttingType;
        SetFileContents(arr);
    }

    internal static void SetFileContents(List<StoreJsonPart> parts)
    {
        lr.ModifyFile("Sim3D.json", parts);
    }
}