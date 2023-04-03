using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ToolsButton : MonoBehaviour
{
    public Button btn;
    private bool isOpen;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (ToolsView.ToolsView.Instance.IsActive)
        {
            ViewHandler.HideOne("CompleteToolMenu");
            isOpen = false;
        }
        else
        {
            ViewHandler.ShowOne("CompleteToolMenu");
            isOpen = true;
        }
    }
}