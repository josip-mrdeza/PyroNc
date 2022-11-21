using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.Debug.Net;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI.Debug;

public class NetworkClient : MonoBehaviour
{
    public Connection Connection;
    public Stopwatch Stopwatch = Stopwatch.StartNew();
    public TextMeshProUGUI Text;
    private void Start()
    {
        Connection = new Connection();
    }

    private async void Update()
    {
        var pos = await Connection.Position;
        var vct = await Connection.VectorChanges;
        if (pos == null)
        {
            Text.text = "Failed to fetch data! - " + Stopwatch.Elapsed.TotalMilliseconds.Round() + "ms";
        }
        else
        {
            Globals.Tool.Position = pos.Value;
            Text.text = Stopwatch.Elapsed.TotalMilliseconds.Round() + "ms";
        }

        if (vct is not null)
        {
            Globals.Tool.Vertices = new List<Vector3>(vct);
        }
        
        Stopwatch.Restart();
        await Task.Delay(Connection.Interval);
    }
}