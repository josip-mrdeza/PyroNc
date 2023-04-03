using System.Collections.Generic;
using Pyro.Nc.Simulation.Workpiece;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class WorkpieceButton : MonoBehaviour
{
    public Button btn;
    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (WorkpieceControl.Instance.View.IsActive)
        {
            ViewHandler.HideOne("WorkpieceEditor");
        }
        else
        {
            ViewHandler.ShowOne("WorkpieceEditor");
        }
    }
}