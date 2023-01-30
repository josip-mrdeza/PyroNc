using System;
using System.Globalization;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Debug;

public class FpsCounter : View
{
    public TextMeshProUGUI fpsText;
    public float deltaTime;
    public long iteration;
    public float longFps;
    public static float averageFps;

    public override void UpdateView() 
    {
        if (fpsText is null)
        {
            return;
        }
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        longFps += fps;
        iteration++;
        if (iteration > 200)
        {
            averageFps = longFps / iteration;
            if (averageFps <= 40)
            {
                Globals.Tool.Values.FastMoveTick = TimeSpan.Zero;
                Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericMessage, "Set Tool.Values.FastMoveTick to 0ms because the fps dropped below 40!"));
            }
            iteration = 0;
            longFps = 0;
        }
        fpsText.text = Mathf.Ceil(fps).Round(1).ToString(CultureInfo.InvariantCulture);
    }
}