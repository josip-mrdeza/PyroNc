using System;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ResetButton : MonoBehaviour
{
    internal static Button _buttonStatic;
    private Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        _buttonStatic = _button;
        _button.onClick.AddListener(() =>
        {
            var tool = Globals.Tool;
            var controller = tool.Workpiece;
            const float c = 0.18823529411764705882352941176471f;
            var color = new Color(c, c, c, 1f);
            
            tool.Vertices = controller.StartingMesh.Vertices.ToList();
            controller.Current.vertices = tool.Vertices.GetInternalArray();

            tool.Colors = Enumerable.Repeat(color, tool.Vertices.Count).ToList();
            controller.Current.colors = tool.Colors.GetInternalArray();
            //Globals.Tool.
        });
    }
}