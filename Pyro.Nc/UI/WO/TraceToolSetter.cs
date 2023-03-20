using System.Collections.Generic;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using UnityEngine.UI;

namespace Pyro.Nc.UI.WO;

public class TraceToolSetter : InitializerRoot
{
    public Toggle Toggle;
    public override void Initialize()
    {
        Toggle = GetComponentInChildren<Toggle>();
        Toggle.isOn = Sim3D.KeepTraceLines;
        Toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(bool val)
    {
        Sim3D.KeepTraceLines = val;
        var arr = CutTypeSetter.lr.ReadFileAs<List<StoreJsonPart>>("Sim3D.json");
        var sjp = arr.Find(x => x.Name == nameof(Sim3D.KeepTraceLines));
        sjp.Value = val;
        CutTypeSetter.SetFileContents(arr);
    }
}