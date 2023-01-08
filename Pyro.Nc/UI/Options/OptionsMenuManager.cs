using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.Nc.UI.Options.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Options;

public class OptionsMenuManager : View
{
    public static OptionsMenuManager Instance;
    public List<OptionBase> LeftOptions;
    public List<OptionBase> RightOptions;
    public Vector2 LeftPoint = new Vector2(-480, 440);
    public Vector2 RightPoint = new Vector2(480, 440);

    //public Button Button;

    public override void Initialize()
    {
        base.Initialize();
        Instance = this;
        LeftOptions = new List<OptionBase>();
        RightOptions = new List<OptionBase>();
        //Button.onClick.AddListener(Test);
    }

    public void Test()
    {
        List<string> options = Enumerable.Repeat(1, 10).Select(x => (x + 3).ToString()).ToList();
        for (int i = 0; i < 10; i++)
        {
            // var opt = AddAsMenuOption<DropdownOption>($"Dropdown {i} ", 300, 50,
            //                                                    i % 2 == 0 ? OptionBase.Side.Left
            //                                                        : OptionBase.Side.Right, 100);
            // opt.Init();
            // opt.Source = options;
            // Console.WriteLine(opt.name);
        }
    }
}