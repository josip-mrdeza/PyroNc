using System;
using System.Globalization;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.UI;
using Pyro.Nc.UI.UI_Screen;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class DistanceTracker : MonoBehaviour
{
    public Vector3 FirstPoint;
    public Vector3 FinalPoint;
    public bool Clicked;
    public float Distance;
    private LineRenderer lr;
    private Vector3[] objs = new Vector3[2];
    [StoreAsJson]
    public static bool ShouldTrackEvent { get; set; }
    private void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        if (!ShouldTrackEvent)
        {
            return;
        }
        ClickHandler.Instance.OnRightClick += () =>
        {
            if (!ShouldTrackEvent)
            {
                return;
            }
            FirstPoint = default;
            FinalPoint = default;
            Clicked = false;
            objs[0] = FirstPoint;
            objs[1] = FinalPoint;
            lr.positionCount = 2;
            lr.SetPositions(objs);
        };
        ClickHandler.Instance.OnLeftDoubleClick += vector3 =>
        {
            if (!ShouldTrackEvent)
            {
                return;
            }
            if (!ViewHandler.Views["3DView"].IsActive)
            {
                return;
            }
            if (Clicked)
            {
                FinalPoint = vector3;
                objs[0] = FirstPoint;
                objs[1] = FinalPoint;
                lr.positionCount = 2;
                lr.SetPositions(objs);
                Distance = Vector3.Distance(FirstPoint, FinalPoint);
                PopupHandler.PopText($"Distance is {Distance.Round(3).ToString(CultureInfo.InvariantCulture)}mm");
                Clicked = false;
            }
            else
            {
                FirstPoint = vector3;
                Clicked = true;
            }
        };
        
    }
}