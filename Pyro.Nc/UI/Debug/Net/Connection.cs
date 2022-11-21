using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Pyro.Nc.UI.Debug.Net;

public class Connection
{
    public HttpClient Client { get; }
    public int Interval { get; set; }
    public bool IsRunning { get; set; }

    public Connection()
    {
        Client = new HttpClient();
        Client.BaseAddress = new Uri("http://192.168.1.200:721/");
        Client.Timeout = TimeSpan.FromMilliseconds(500);
        Interval = 50;
        IsRunning = true;
    }

    public Task<Vector3?> Position => GetPosition();
    public Task<Vector3[]> VectorChanges => GetVectorChanges();

    private async Task<Vector3?> GetPosition()
    {
        try
        {
            var json = await Client.GetStringAsync("ToolPosition");
            var obj = JsonUtility.FromJson<Vector3>(json);

            return obj;
        }
        catch
        {
        }

        return null;
    }

    private async Task<Vector3[]> GetVectorChanges()
    {
        try
        {
            var json = await Client.GetStringAsync("Workpiece");
            var obj = JsonUtility.FromJson<Vector3[]>(json);

            return obj;
        }
        catch
        {
        }

        return null;
    }
}