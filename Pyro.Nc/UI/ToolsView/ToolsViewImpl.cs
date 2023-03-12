using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.UI.Options;
using Pyro.Nc.UI.Options.Implementations;
using UnityEngine;

namespace Pyro.Nc.UI.ToolsView;

public class ToolsViewImpl : MonoBehaviour
{
    public static ToolsViewImpl Instance;
    public OptionBase.Side _side;
    public List<OptionBase> Options;
    public void Start()
    {
        if (Options != null)
        {
            foreach (var optionBase in Options)
            {
                optionBase.SetSize(Vector2.zero);
                DestroyImmediate(optionBase);
            }
        }

        Instance = this;
        var go = gameObject;
        LocalRoaming r = LocalRoaming.OpenOrCreate("PyroNc");
        var tc = r.ReadFileAs<ToolConfiguration[]>("ToolConfig.json");
        Options = new List<OptionBase>();
        for (var i = 0; i < tc.Length; i++)
        {
            var tool = tc[i];
            var opt = go.AddAsMenuOption<ButtonTextOption>($"T{tool.Index} {tool.Name}", 600, 100, _side, init: true);
            Options.Add(opt);
            opt.OnClick += SetToolInOptions;
        }
    }

    private void SetToolInOptions(OptionBase optionBase)
    {
        LocalRoaming r = LocalRoaming.OpenOrCreate("PyroNc");
        var arr = r.ReadFileAs<ToolConfiguration[]>("ToolConfig.json");
        var str = string.Join(" ", optionBase.name.Split(' ').Skip(1));
        ToolOptionsManager.Instance.Set(arr.First(x => x.Name == str));
    }
}