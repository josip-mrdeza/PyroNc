using System.Linq;
using Pyro.IO;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class CloseButton : View
{
    public static CloseButton Instance;
    public Button _button;
    private void Start()
    {
        base.Initialize();
        Instance = this;
        _button.onClick.AddListener(() =>
        {
            Globals.GCodeInputHandler.LoadText(string.Empty, null);
            Globals.GCodeInputHandler.MarkDirty();
            ViewHandler.ShowOne(Globals.Loader.Id);
        });
    }
}