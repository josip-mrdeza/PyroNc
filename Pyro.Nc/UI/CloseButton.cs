using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class CloseButton : InitializerRoot
{
    public static CloseButton Instance;
    public Button _button;

    public override void Initialize()
    {
        base.Initialize();
        Instance = this;
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            Globals.GCodeInputHandler.LoadText(string.Empty, null);
            Globals.GCodeInputHandler.MarkDirty();
            ViewHandler.ShowOne(Globals.Loader.Id);
        });
    }
}