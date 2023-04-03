using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pyro.Nc.Simulation.Machines;

public class ClickHandler : MonoBehaviour
{
    public static ClickHandler Instance;
    int clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;
    private Camera main;
    private void Awake()
    {
        Instance = this;
        main = Camera.main;
    }

    public bool DoubleClick()
    {
        if (Input.GetMouseButtonDown(0) && Input.GetKeyDown(KeyCode.LeftControl))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        return false;
    }
    
    public bool LeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        return false;
    }

    public bool RightClick()
    {
        return Input.GetMouseButtonDown(1);
    }

    public void Update()
    {
        if (DoubleClick())
        {
            var cursorObj = Input.mousePosition;
            var ray = main.ScreenPointToRay(cursorObj);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.green, 1f);
            
            Physics.Raycast(ray, out var rch, float.PositiveInfinity);
            OnControlDoubleClick?.Invoke(rch.point);
        }
        if (LeftClick())
        {
            var cursorObj = Input.mousePosition;
            var ray = main.ScreenPointToRay(cursorObj);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.green, 1f);
            
            Physics.Raycast(ray, out var rch, float.PositiveInfinity);
            OnLeftDoubleClick?.Invoke(rch.point); 
        }
        if (RightClick())
        {
            OnRightClick?.Invoke();
        }
    }

    public event Action<Vector3> OnControlDoubleClick;
    public event Action<Vector3> OnLeftDoubleClick;
    public event Action OnRightClick;
}