using System;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI;

public class ResetButton : MonoBehaviour
{
    public static Button Instance;
    private Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        Instance = _button;
        _button.onClick.AddListener(async () =>
        {
            var tool = Globals.Tool;
            var controller = tool.Workpiece;
            const float c = 0.18823529411764705882352941176471f;
            var color = new Color(c, c, c, 1f);
            
            tool.Vertices = controller.StartingMesh.Vertices.ToList();
            controller.Current.vertices = tool.Vertices.GetInternalArray();

            tool.Colors = Enumerable.Repeat(color, tool.Vertices.Count).ToList();
            controller.Current.colors = tool.Colors.GetInternalArray();
            tool.Values.IsReset = true;
            tool.Values.IsPaused = false;
            await tool.EventSystem.FireAsync("ProgramEnd");
            //Globals.Tool.
        });
    }
}