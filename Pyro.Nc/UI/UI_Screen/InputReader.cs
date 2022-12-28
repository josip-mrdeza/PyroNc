using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen;

public class InputReader : MonoBehaviour
{
    public string Name;
    [SerializeField] private TMP_InputField Field;
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
    }
}