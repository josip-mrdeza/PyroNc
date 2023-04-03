using Pyro.Nc.Simulation;
using Pyro.Nc.UI.WO;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ProgramsButton : MonoBehaviour
{
    public Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (Globals.Loader is null)
        {
            Globals.Console.Push("Loader is null!");
        }
        if (Globals.Loader.IsActive)
        {
            ViewHandler.HideOne(Globals.Loader.name);
        }
        else
        {
            ViewHandler.ShowOne(Globals.Loader.name);
        }
    }
}