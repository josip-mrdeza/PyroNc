using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options.Implementations;

public class ButtonOption : OptionBase
{
    private static GameObject Prefab;
    private Button _button;
    private TextMeshProUGUI _text;
    
    public static ButtonOption LoadPrefab(OptionsMenuManager manager)
    {
        if (Prefab == null)
        {
            Prefab = Resources.Load<GameObject>("Options/ButtonOptionPrefab");
            Prefab.SetActive(false);
        }

        var obj = Instantiate(Prefab, manager.transform);
        obj.SetActive(true);
        var button = obj.AddComponent<ButtonOption>();
        return button;
    }
    
    public override void Init()
    {
        _button = gameObject.GetComponentInChildren<Button>();
        _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _text.text = name;
        _button.onClick.AddListener(OnClick);
    }

    public event UnityAction OnClick;
}