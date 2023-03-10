using System;
using System.Globalization;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI;

public class LoadingScreenView : View
{
    public TextMeshProUGUI loadingDescriptor;
    public TextMeshProUGUI additionalDescriptor;
    public Transform ImageTr;
    public static LoadingScreenView Instance;
    public float step = 10f;
    public override void Initialize()
    {
        base.Initialize();
        Instance = this;
        SetText("Starting...");
        var manager = Globals.Initializer;
        manager.OnLoadedScript += OnLoadedScriptHandler;
        manager.OnCompletedInitialization += OnCompletedInitializationHandler;
        //Globals.OnLog += OnLogHandler;
    }

    public virtual void FixedUpdate()
    {
        ImageTr.Rotate(new Vector3(0, 0, 1), step);
    }

    public virtual void OnLoadedScriptHandler(InitializerRoot root, TimeSpan span, int index)
    {
        SetText($"Loaded {root.name} in {span.TotalMilliseconds.Round().ToString(CultureInfo.InvariantCulture)}ms.");
        SetAdditionalText($"Script: {index.ToString()}/{Globals.Initializer.Scripts.Count.ToString()}");
    }

    public virtual void OnCompletedInitializationHandler(MonoInitializer initializer, TimeSpan span)
    {
        SetText("Completed initialization!");
        SetText("Removing event subscribers...");
        initializer.OnLoadedScript -= OnLoadedScriptHandler;
        initializer.OnCompletedInitialization -= OnCompletedInitializationHandler;
        Globals.OnLog -= OnLogHandler;
        SetText("Done!");
        Hide();
    }

    private void OnLogHandler(string str)
    {
        SetText(str);
    }

    public void SetText(string text)
    {
        loadingDescriptor.text = text;
    }

    public void SetAdditionalText(string text)
    {
        additionalDescriptor.text = text;
    }
}