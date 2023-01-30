using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options.Implementations;

public class InputOption : OptionBase
{
    private static GameObject Prefab;
    private TMP_InputField _input;
    private TextMeshProUGUI _inputPlaceholder;
    private TextMeshProUGUI _text;

    public string Text
    {
        get => _input.text;
        set => _input.text = value;
    }

    public static InputOption LoadPrefab(OptionsMenuManager manager)
    {
        if (Prefab == null)
        {
            Prefab = Resources.Load<GameObject>("Options/InputOptionPrefab");
            Prefab.SetActive(false);
        }
        var obj = Instantiate(Prefab, manager.transform);
        obj.SetActive(true);
        var input = obj.AddComponent<InputOption>();
        return input;
    }
    
    public override void Init()
    {
        _input = gameObject.GetComponentInChildren<TMP_InputField>();
        _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _text.text = name;
        _input.onEndEdit.AddListener(OnEndEdit);
    }

    public void Init<T>() where T : new()
    {
        Init();
        _inputPlaceholder = _input.transform.Find("Text Area").transform.Find("Placeholder").GetComponent<TextMeshProUGUI>();
        _inputPlaceholder.text = (new T()).ToString();
    }

    public event UnityAction<string> OnEndEdit;
}