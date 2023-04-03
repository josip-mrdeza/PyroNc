using System.Collections.Generic;
using Pyro.Nc.UI.WO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class WorkOffsetsButton : MonoBehaviour
{
    public Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (WorkOffsetView.Instance.IsActive)
        {
            ViewHandler.HideOne("WORK_OFFSETS");
        }
        else
        {
            ViewHandler.ShowOne("WORK_OFFSETS");
        }
    }
}