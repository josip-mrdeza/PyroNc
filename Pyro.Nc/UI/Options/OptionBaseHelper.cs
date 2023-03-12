using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pyro.Nc.UI.Options;

public static class OptionBaseHelper
{
    private static readonly object[] EmptyObjects = Array.Empty<object>();
    
    public static T AddAsMenuOption<T>(this GameObject parent, string name, float width, float height, OptionBase.Side side,
        float additionalSpacing = float.NaN, float downwardShift = float.NaN, bool init=false) where T : OptionBase
    {
        var manager = parent.GetComponent<OptionsMenuManager>();
        if (manager == null)
        {
            manager = parent.AddComponent<OptionsMenuManager>();
            manager.name = $"manager_{Random.Range(0, int.MaxValue)}";
            manager.Initialize();
        }
        else
        {
            if (!manager.IsInitialized)
            {
                manager.Initialize();
            }
        }
        var method = typeof(T).GetMethod("LoadPrefab");
        var comp = method.Invoke(null, new object[]{manager}) as T;
        if (parent != null)
        {
            comp.gameObject.transform.SetParent(parent.transform, false);
        }
        var transform = comp.transform;
        var rectTr = (RectTransform) transform;
        comp._uiTransform = rectTr;
        comp.name = name;
        comp.SetSize(new Vector2(width, height));
        Vector2 target;
        if (float.IsNaN(additionalSpacing))
        {
            additionalSpacing = 0;
        }

        if (float.IsNaN(downwardShift))
        {
            downwardShift = 0;
        }
        if (side == OptionBase.Side.Left)
        {
            target = manager.LeftPoint;
            target.x += width / 2;
            if (manager.LeftOptions.Count > 0)
            {
                target.y -= manager.LeftOptions.Sum(x => x._height);
            }

            manager.LeftOptions.Add(comp);
        }
        else if (side == OptionBase.Side.Middle)
        {
            target = new Vector2(0, 0);
            target.y = manager.RightPoint.y;
            if (manager.MiddleOptions.Count > 0)
            {
                target.y -= manager.MiddleOptions.Sum(x => x._height);
            }

            manager.MiddleOptions.Add(comp);
        }
        else
        {
            target = manager.RightPoint;
            target.x -= width / 2;
            if (manager.RightOptions.Count > 0)
            {
                target.y -= manager.RightOptions.Sum(x => x._height);
            }
            manager.RightOptions.Add(comp);
        }
        target.y -= height / 2;
        target.y -= downwardShift;
        comp._height += additionalSpacing;
        comp.Move(target);
        if (init)
        {
            comp.Init();
        }
        return comp;
    }

    public static T AddAsMenuOption<T>(this OptionsMenuManager manager, string name, float width, float height,
        OptionBase.Side side, float additionalSpacing = float.NaN, float downwardShift = float.NaN) where T : OptionBase
    {
        return manager.gameObject.AddAsMenuOption<T>(name, width, height, side, additionalSpacing, downwardShift);
    }
}