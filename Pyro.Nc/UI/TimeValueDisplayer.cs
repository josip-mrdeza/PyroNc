using System;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI;

public class TimeValueDisplayer : MonoBehaviour
{
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Value;
    public TimeSpan Time {
        get
        {
            return _time;
        }
        set
        {
            _time = value;
            var days = _time.Days;
            var hours = _time.Hours;
            var minutes = _time.Minutes;
            var sec = _time.Seconds;
            var ms = _time.Milliseconds;
            Value.text = $"{days} days\n" +
                $"{hours} hours\n" +
                $"{minutes} minutes\n" +
                $"{sec} seconds\n" +
                $"{ms} milliseconds";
        }
    }

    private TimeSpan _time;

    private void Start()
    {
        Description = transform.Find("Description").GetComponent<TextMeshProUGUI>();
        Value = transform.Find("Value").GetComponent<TextMeshProUGUI>();
    }
}