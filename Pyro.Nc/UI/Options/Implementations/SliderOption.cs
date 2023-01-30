using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options.Implementations;

public class SliderOption : OptionBase
{
    private static GameObject Prefab;
    private Slider _sliderScript;
    private TextMeshProUGUI _text;
    public float minValue;
    public float maxValue;

    public static SliderOption LoadPrefab(OptionsMenuManager manager)
    {
        if (Prefab == null)
        {
            Prefab = Resources.Load<GameObject>("Options/SliderOptionPrefab");
            Prefab.SetActive(false);
        }

        var obj = Instantiate(Prefab, manager.transform);
        obj.SetActive(true);
        var slider = obj.AddComponent<SliderOption>();
        return slider;
    }

    public override void Init()
    {
        _sliderScript = gameObject.GetComponent<Slider>();
        _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _sliderScript.minValue = minValue;
        _sliderScript.maxValue = maxValue;
        _text.text = name;
    }
}