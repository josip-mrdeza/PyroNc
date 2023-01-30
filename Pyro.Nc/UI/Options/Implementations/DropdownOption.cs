using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Pyro.Nc.UI.Options.Implementations;

public class DropdownOption : OptionBase
{
    public static GameObject Prefab;
    private TMP_Dropdown _dropdown;
    private TextMeshProUGUI _text;
    private List<string> _source;
    
    public List<TMP_Dropdown.OptionData> ExtendedSource
    {
        get => _dropdown.options;
        set{
           _dropdown.ClearOptions();
           _dropdown.AddOptions(value);
        }
    }
    
    public List<string> Source
    {
        get => _source ??= _dropdown.options.Select(x => x.text).ToList();
        set{
            _dropdown.ClearOptions();
            _dropdown.AddOptions(value);
            _source = _dropdown.options.Select(x => x.text).ToList();
        }
    }
    
    public static DropdownOption LoadPrefab(OptionsMenuManager manager)
    {
        if (Prefab == null)
        {
            Prefab = Resources.Load<GameObject>("Options/DropdownOptionPrefab");
            Prefab.SetActive(false);
        }

        var obj = Instantiate(Prefab, manager.transform);
        obj.SetActive(true);
        var dropdown = obj.AddComponent<DropdownOption>();
        return dropdown;
    }
    
    public override void Init()
    {
        _dropdown = gameObject.GetComponentInChildren<TMP_Dropdown>();
        _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _text.text = name;
        _dropdown.onValueChanged.AddListener(OnChangeSelectionIndex);
        _dropdown.onValueChanged.AddListener(index =>
        {
            if (OnChangeSelection == null)
            {
                return;
            }
            var option = _dropdown.options[index];
            OnChangeSelection(option);
        });
    }

    public event UnityAction<int> OnChangeSelectionIndex;
    public event UnityAction<TMP_Dropdown.OptionData> OnChangeSelection;
}