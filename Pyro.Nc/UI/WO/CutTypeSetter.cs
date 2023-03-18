using System;
using System.Linq;
using System.Reflection;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using TMPro;

namespace Pyro.Nc.UI.WO;

public class CutTypeSetter : InitializerRoot
{
    public TMP_Dropdown Dropdown;
    public override void Initialize()
    {
        Dropdown = GetComponent<TMP_Dropdown>();
        Dropdown.ClearOptions();
        var type = typeof(CutType);
        var enums = type.GetEnumNames();
        Dropdown.AddOptions(enums.ToList());
        Dropdown.onValueChanged.Invoke((int) Sim3D.CuttingType);
        Dropdown.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(int index)
    {
        Sim3D.CuttingType = (CutType)index;
        Push($"[CutTypeSetter]: Set cutting type to: {((CutType)index).ToString()}");
    }
}