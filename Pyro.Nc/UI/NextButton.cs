using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class NextButton : View
{
    public Button btn;
    public TextMeshProUGUI Text;
    public List<GameObject> OriginalItems;
    public List<GameObject> NextItems;
    private State ButtonState;

    public override void Show()
    {
        OnClick();
    }

    public override void Hide()
    {
        OnClick();
    }

    private void Start()
    {
        btn = GetComponent<Button>();
        Text = GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(OnClick);
        ButtonState = State.Previous;
        base.Key = KeyCode.Tab;
        InitializeComplete().Wait();
    }

    private void OnClick()
    {
        if (ButtonState == State.Previous)
        {
            foreach (var obj in OriginalItems)
            {
                obj.SetActive(true);
            }
            foreach (var obj in NextItems)
            {
                obj.SetActive(false);
            }
            ButtonState = State.Next;
            Text.text = "Next";
        }
        else
        {
            foreach (var obj in OriginalItems)
            {
                obj.SetActive(false);
            }
            foreach (var obj in NextItems)
            {
                obj.SetActive(true);
            }
            ButtonState = State.Previous;
            Text.text = "Previous";
        }
    }
    
    private enum State
    {
        Previous,
        Next
    }
}