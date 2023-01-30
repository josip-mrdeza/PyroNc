using System;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI;

public class ValueDisplayer : MonoBehaviour
{
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Value;

    private void Start()
    {
        Description = transform.Find("Description").GetComponent<TextMeshProUGUI>();
        Value = transform.Find("Value").GetComponent<TextMeshProUGUI>();
    }
}