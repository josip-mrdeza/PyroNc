using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Programs;

public class GraphicHolder : MonoBehaviour
{
    public Graphic Background;
    public Graphic Checkmark;

    public static GraphicHolder Instance;

    private void Awake()
    {
        Instance = this;
    }
}