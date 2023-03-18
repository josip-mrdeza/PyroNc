using System.Reflection;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using TMPro;

namespace Pyro.Nc.UI.WO;

public class WaitTimeSetter : InitializerRoot
{
    public TMP_InputField Input;
    private PropertyInfo info;
    public override void Initialize()
    {
        Input = GetComponent<TMP_InputField>();
        Input.onValueChanged.Invoke(Sim3D.WaitStep.ToString());
        Input.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(string txt)
    {
        if (!float.TryParse(txt, out var f))
        {
            return;
        }

        Sim3D.WaitStep = f;
    }
}