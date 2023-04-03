using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pyro.Nc.UI.Debug;

public static class CutViewer
{
    public static void ShowPoint(this Vector3 point, float seconds)
    {
        UnityEngine.Debug.DrawLine(point - new Vector3(0, 0.01f, 0), point + new Vector3(0, 0.01f, 0), Color.blue, seconds);
        // UnityEngine.Debug.DrawLine(point - new Vector3(1, 0, 0), point + new Vector3(1, 0, 0), Color.blue, seconds);
        // UnityEngine.Debug.DrawLine(point - new Vector3(0, 0, 1), point + new Vector3(0, 0, 1), Color.blue, seconds);
    }
    public static void ShowPoint(this Vector3 point, float seconds, Color color)
    {
        UnityEngine.Debug.DrawLine(point - new Vector3(0, 0.01f, 0), point + new Vector3(0, 0.01f, 0), color, seconds);
        // UnityEngine.Debug.DrawLine(point - new Vector3(1, 0, 0), point + new Vector3(1, 0, 0), Color.blue, seconds);
        // UnityEngine.Debug.DrawLine(point - new Vector3(0, 0, 1), point + new Vector3(0, 0, 1), Color.blue, seconds);
    }
}