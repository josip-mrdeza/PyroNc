using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen;

public class InputReader : MonoBehaviour
{
    public string Name;
    [SerializeField] private TMP_InputField Field;

    public string Text
    {
        get => Field.text;
        set => Field.text = value;
    }
    private void Start()
    {
        Field ??= GetComponentInChildren<TMP_InputField>();
        if (!InputHolder.Readers.ContainsKey(Name))
        {
           InputHolder.Readers.Add(Name, this); 
        }
        else
        {
            throw new ArgumentException($"Name '{Name}' was already taken!");
        }

        Field.onValueChanged.AddListener(s => OnChanged?.Invoke(s));
        Field.onDeselect.AddListener(s => OnChanged?.Invoke(s));
    }

    public void InvokeEvent(string s)
    {
        OnChanged?.Invoke(s);
    }

    public event UnityAction<string> OnChanged;
}