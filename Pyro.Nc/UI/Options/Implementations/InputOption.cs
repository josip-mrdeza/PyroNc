using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options.Implementations;

public class InputOption : OptionBase
{
    private static GameObject Prefab;
    private TMP_InputField _input;
    private TextMeshProUGUI _text;
    
    public string Value => _input.text;

    public static InputOption LoadPrefab()
    {
        if (Prefab == null)
        {
            Prefab = Resources.Load<GameObject>("Options/InputOptionPrefab");
            Prefab.SetActive(false);
        }

        var obj = Instantiate(Prefab, OptionsMenuManager.Instance.gameObject.transform);
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

    public event UnityAction<string> OnEndEdit;
}