using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.UI.Options.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options;

public class OptionsMenuManager : View
{ 
    public List<OptionBase> LeftOptions;
    public List<OptionBase> MiddleOptions;
    public List<OptionBase> RightOptions;
    public Vector2 LeftPoint;
    public Vector2 RightPoint;

    //public Button Button;

    public override void Initialize()
    {
        base.Initialize();
        LeftOptions = new List<OptionBase>();
        MiddleOptions = new List<OptionBase>();
        RightOptions = new List<OptionBase>();
        var rectTr = gameObject.transform as RectTransform;
        var rect = rectTr.sizeDelta;
        LeftPoint = new Vector2(-rect.x / 2, rect.y/2);
        RightPoint = new Vector2(rect.x / 2, rect.y/2);
        //Button.onClick.AddListener(Test);
    }

    public void Refresh()
    {
        var left = LeftOptions.Sum(x => x.Height) / 2;
        foreach (var option in LeftOptions)
        {
            option.Position += new Vector2(0, left);
        }

        var middle = MiddleOptions.Sum(x => x.Height) / 2;
        foreach (var option in MiddleOptions)
        {
            option.Position += new Vector2(0, middle);
        }

        var right = RightOptions.Sum(x => x.Height) / 2;
        foreach (var option in RightOptions)
        {
            option.Position += new Vector2(0, right);
        }
    }

    public override void UpdateView()
    {
        Refresh();
    }
}