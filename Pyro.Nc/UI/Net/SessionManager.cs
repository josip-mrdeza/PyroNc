using System;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Net;
using UnityEngine;

namespace Pyro.Nc.UI.Net;

public class SessionManager : MonoBehaviour
{
    private const string RaspberryPi = "http://pi:5000/session/all";
    private const string GlobalAddress = "https://pyronetserver.azurewebsites.net/session/all";
    public string Address { get; private set; }
    private async void Start()
    {
        var isPiOnline = await RaspberryPi.Ping();
        Globals.Console.Push(isPiOnline?"Found Local RaspberryPi Server running!":"No RaspberryPi Server running, checking global...");
        Address = isPiOnline ? "http://pi:5000/" : "https://pyronetserver.azurewebsites.net/";
        var isGlobalOnline = await GlobalAddress.Ping();
        Globals.Console.Push(isGlobalOnline?"Found Global RaspberryPi Server running!":"No Global RaspberryPi Server running!");
        if (!isPiOnline && !isGlobalOnline)
        {
            var msg = "No suitable internet address found, are you connected to the internet?";
            Globals.Comment.PushComment(msg, Color.red);
        }
        else
        {
            Globals.Comment.PushComment($"Session manager connected to host: '{Address}'!", Color.green);
        }
        
    }
}