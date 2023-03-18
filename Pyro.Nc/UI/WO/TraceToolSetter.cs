using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using UnityEngine.UI;

namespace Pyro.Nc.UI.WO;

public class TraceToolSetter : InitializerRoot
{
    public Toggle Toggle;
    public override void Initialize()
    {
        Toggle = GetComponent<Toggle>();
        Toggle.onValueChanged.Invoke(Sim3D.KeepTraceLines);
        Toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(bool val)
    {
        Sim3D.KeepTraceLines = val;
    }
}