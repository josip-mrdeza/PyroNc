using System;
using System.Collections.Generic;
using System.Reflection;
using Pyro.IO;
using Pyro.Nc.UI.Options;
using Pyro.Nc.UI.Options.Implementations;
using UnityEngine;

namespace Pyro.Nc.UI.Cycles;

public static class CycleCreator
{
    public static readonly Dictionary<CycleType, OptionBase[]> CachedCycleCreators = new Dictionary<CycleType, OptionBase[]>();

    public enum CycleType
    {
        Cycle81,
        Cycle82,
        Cycle83
    }
    
    public static void OpenCycleCreatorOnGameObject(this GameObject go, CycleType cycleType)
    {
        if (CachedCycleCreators.ContainsKey(cycleType))
        {
            var options = CachedCycleCreators[cycleType];
            foreach (var optionBase in options)
            {
                var inputOption = optionBase.CastInto<InputOption>();
                inputOption.Text = string.Empty;
            }
            return;
        }
        var fullName = cycleType.ToString().ToUpperInvariant();
        var type = Type.GetType($"Pyro.Nc.Parsing.Cycles.{fullName}");
        var field = type.GetField("ParameterDescriptions", BindingFlags.Public | BindingFlags.Static);
        var array = field.GetValue(null).CastInto<string[]>();
        var optBaseArr = new OptionBase[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            var s = array[i];
            var io = go.AddAsMenuOption<InputOption>(s, 300, 100, OptionBase.Side.Middle, 0, 0);
            io.Init<float>();
            optBaseArr[i] = io;
        }

        var manager = go.GetComponent<OptionsMenuManager>();
        manager.Refresh();
        
        CachedCycleCreators.Add(cycleType, optBaseArr);
        View view = go.GetComponent<View>();
        if (view == null)
        {
            view = go.AddComponent<View>();
            view.Initialize();
        }
        view.RefreshChildObjects();
        view.Show();
    }

}