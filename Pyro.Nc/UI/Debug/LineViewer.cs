using System;
using Pyro.Math;
using UnityEngine;

namespace Pyro.Nc.UI.Debug;

public class LineViewer : MonoBehaviour
{
    private Vector3[] Points;
    public bool IsActive;
    public bool IsDirty;
    public LineRenderer Renderer;
    public float Radius;
    public Vector3 Position;

    public void Init()
    {
        IsActive = true;
        Radius = 2;
        Renderer = gameObject.AddComponent<LineRenderer>();
        Renderer.useWorldSpace = false;
        Renderer.positionCount = 360;
        Renderer.startWidth = 0.25f;
        Renderer.endWidth = 0.25f;
        Renderer.material = Resources.Load<Material>("Mat0");
        Points = new Vector3[360];
    }

    public virtual void Update()
    {
        if (IsDirty)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (IsActive)
        {
            Renderer.forceRenderingOff = false;
            Renderer.positionCount = 360;
            for (int i = 0; i < 360; i++)
            {
                var horizontal = i.Cos() * Radius;
                var vertical = i.Sin() * Radius;
                Vector3 v;
                Vector3 h;
                if (Renderer.useWorldSpace)
                {
                    var x = horizontal + Position.x;
                    v = new Vector3(x, vertical + Position.y, Position.z);
                    h = new Vector3(x, Position.y, vertical + Position.z);
                }
                else
                {
                    v = new Vector3(horizontal, vertical);
                    h = new Vector3(horizontal, 0, vertical);
                }
                var ii = i / 2;
                Points[ii] = h;
                Points[(ii + 180)] = v;
            }
            Renderer.SetPositions(Points); 
        }
        else
        {
            Renderer.enabled = false;
        }
    }

    private Vector3[] arr = new Vector3[0];
}