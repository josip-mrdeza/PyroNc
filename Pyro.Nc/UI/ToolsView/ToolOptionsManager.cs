using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.ToolsView;

public class ToolOptionsManager : MonoBehaviour
{
    public static ToolOptionsManager Instance;
    public ToolConfiguration CurrentToolConfiguration;
    public LocalRoaming Roaming;
    public TMP_InputField Tool_ID;
    public TMP_InputField Mesh_ID;
    public TMP_InputField Radius;
    public TMP_InputField ColorA;
    public TMP_InputField ColorR;
    public TMP_InputField ColorG;
    public TMP_InputField ColorB;
    public Image Img;
    public TMP_InputField VerticalMargin;
    public TMP_InputField ToolLength;
    public Button _Button;
    public TextMeshProUGUI BtnText;
    public Button ClearButton;

    private void Start()
    {
        Instance = this;
        Roaming = LocalRoaming.OpenOrCreate("PyroNc");
        BtnText = _Button.GetComponentInChildren<TextMeshProUGUI>();

        _Button.onClick.AddListener(SaveToolConfig);
        ClearButton.onClick.AddListener(ClearAll);
    }

    private void ClearAll()
    {
        Set(new ToolConfiguration(string.Empty, string.Empty, 0,-1,0,0, 0,0,0,255));
        CurrentToolConfiguration = null;
    }

    public void Set(ToolConfiguration config)
    {
        CurrentToolConfiguration = config;
        Tool_ID.text = config.Name;
        Mesh_ID.text = config.Id;
        Radius.text = config.Radius.ToString(CultureInfo.InvariantCulture);
        ColorA.text = (config.A).ToString(CultureInfo.InvariantCulture);
        ColorR.text = (config.R).ToString(CultureInfo.InvariantCulture);
        ColorG.text = (config.G).ToString(CultureInfo.InvariantCulture);
        ColorB.text = (config.B).ToString(CultureInfo.InvariantCulture);
        VerticalMargin.text = config.VerticalMargin.ToString(CultureInfo.InvariantCulture);
        ToolLength.text = config.ToolLength.ToString(CultureInfo.InvariantCulture);
        Img.color = config.GetColor();
    }

    private void LateUpdate()
    {
        if (CurrentToolConfiguration == null)
        {
            BtnText.text = "Insert";
        }
        else
        {
            BtnText.text = "Save";
        }
        try
        {
            var a = float.Parse(ColorA.text.FixEmptyString<float>());
            var r = float.Parse(ColorR.text.FixEmptyString<float>());
            var g = float.Parse(ColorG.text.FixEmptyString<float>());
            var b = float.Parse(ColorB.text.FixEmptyString<float>());
            Img.color = new Color(r / 255f,g / 255f, b / 255f, a / 255f);
        }
        catch (Exception e)
        {
            //ignore
        }
    }

    private void SaveToolConfig()
    {
        if (CurrentToolConfiguration == null)
        {
            InsertToolConfig();
            return;
        }
        try
        {
            var conf = CurrentToolConfiguration;
            conf.Id = Mesh_ID.text;
            conf.Radius = float.Parse(Radius.text);
            conf.VerticalMargin = float.Parse(VerticalMargin.text);
            conf.ToolLength = float.Parse(ToolLength.text);
            conf.A = float.Parse(ColorA.text);
            conf.R = float.Parse(ColorR.text);
            conf.G = float.Parse(ColorG.text);
            conf.B = float.Parse(ColorB.text);
            var arr = Roaming.ReadFileAs<List<ToolConfiguration>>("ToolConfig.json");
            var index = arr.FindIndex(x => x.Name == conf.Name);
            //var lr = LocalRoaming.OpenOrCreate("PyroNc\\ToolBackups");
            //lr.AddFile($"ToolConfig_Backup_{DateTime.Now.ToShortDateString()}.json", arr);
            arr[index] = conf;
            Roaming.ModifyFile("ToolConfig.json", arr);
            MachineBase.CurrentMachine.ToolControl.Manager.Tools = arr.ToList();
            foreach (var tool in MachineBase.CurrentMachine.ToolControl.Manager.Tools)
            {
                tool.RefreshToolLength();
            }
        }
        catch (Exception e)
        {
            //ignore
        }
    }

    private void InsertToolConfig()
    {
        try
        {
            var conf = new ToolConfiguration(Tool_ID.text, Mesh_ID.text, 0, 0, 0, 0, 0, 0, 0, 0);
            conf.Radius = float.Parse(Radius.text.FixEmptyString<float>());
            conf.A = float.Parse(ColorA.text.FixEmptyString<float>());
            conf.R = float.Parse(ColorR.text.FixEmptyString<float>());
            conf.G = float.Parse(ColorG.text.FixEmptyString<float>());
            conf.B = float.Parse(ColorB.text.FixEmptyString<float>());
            conf.VerticalMargin = float.Parse(VerticalMargin.text.FixEmptyString<float>());
            conf.ToolLength = float.Parse(ToolLength.text.FixEmptyString<float>());
            var arr = Roaming.ReadFileAs<List<ToolConfiguration>>("ToolConfig.json");
            conf.Index = arr.Count;
            if (arr.Exists(x => x.Name == conf.Name))
            {
                PopupHandler.PopText("A tool with the same name already exists in the database!");

                return;
            }
            arr.Add(conf);
            Roaming.ModifyFile("ToolConfig.json", arr);
            CurrentToolConfiguration = conf;
            ToolsViewImpl.Instance.Start();
        }
        catch (Exception e)
        {
            //ignore
        }
    }
}