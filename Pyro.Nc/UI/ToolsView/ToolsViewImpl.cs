using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public async void Start()
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
        if (!r.Exists("ToolConfig.json"))
        {
            for (int i = 0;!r.Exists("ToolConfig.json") || i < 5; i++)
            {
                await Task.Delay(100);
            }
        }
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