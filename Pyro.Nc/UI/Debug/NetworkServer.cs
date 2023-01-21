using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI.Debug;

public class NetworkServer : MonoBehaviour
{
    public HttpListener Listener = new HttpListener();

    private void Start()
    {
        Listener.Prefixes.Add("http://+:721/");
        Listener.Start();
        /*Task.Run(async () =>
        {
            while (Listener.IsListening)
            {
                var context = await Listener.GetContextAsync();
                var url = context.Request.RawUrl;
                byte[] data = null;
                if (url.EndsWith("ToolPosition"))
                {
                    var pos = PyroDispatcher.ExecuteOnMain(() => Globals.Tool.Position);
                    data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(pos));
                }
                else
                {
                    data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(PyroDispatcher.ExecuteOnMain(() => Globals.Tool.Vertices)));
                }

                await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                context.Response.Close();
            }
        });*/
    }

    private async void Update()
    {
        var context = await Listener.GetContextAsync();
        var url = context.Request.RawUrl;
        byte[] data = null;
        if (url.EndsWith("ToolPosition"))
        {
            var pos = Globals.Tool.Position;
            data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(pos));
        }
        else
        {
            data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(Globals.Tool.Vertices));
        }

        await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
        context.Response.Close();
    }
}