using Pyro.Nc.Simulation;
using Pyro.Nc.UI.WO;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ExitButton : MonoBehaviour
{
    public Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        ViewHandler.ShowOne("3DView");
    }
}