using System;
using System.Threading.Tasks;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.Exceptions;

public class NotifyWarning : MonoBehaviour
{
    private async void Start()
    {
        Amtr = GetComponent<Animator>();
        Text = GetComponentInChildren<TextMeshProUGUI>();
        await Task.Delay(1000);
        Amtr.Play("SlideClip");
        Text.text = "All modules loaded!";
        await Task.Delay(500);
        //reverse, idk how
        
        Amtr.Play("SlideClip");
    }

    private Animator Amtr;
    public TextMeshProUGUI Text;
}