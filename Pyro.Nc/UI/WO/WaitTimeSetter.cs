using System.Collections.Generic;
using System.Reflection;
using Pyro.IO;
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
        Input = GetComponentInChildren<TMP_InputField>();
        Input.text = Sim3D.WaitStep.ToString();
        Input.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(string txt)
    {
        if (!float.TryParse(txt, out var f))
        {
            return;
        }

        Sim3D.WaitStep = f;
        var arr = CutTypeSetter.lr.ReadFileAs<List<StoreJsonPart>>("Sim3D.json");
        var sjp = arr.Find(x => x.Name == nameof(Sim3D.WaitStep));
        sjp.Value = f;
        CutTypeSetter.SetFileContents(arr);
    }
}