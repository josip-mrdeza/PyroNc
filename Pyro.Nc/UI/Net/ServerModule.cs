using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using TinyClient;
using TinyServer;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Net;

public class ServerModule : View
{
    public PyroReceiver Receiver;
    public Button StartListeningButton;
    public TextMeshProUGUI StatusText;
    public List<GameObject> Devices;
    public GameObject DevicePrefab;

    public override void Initialize()
    {
        base.Initialize();
        Devices = new List<GameObject>();
        StartListeningButton.onClick.AddListener(StartServer());
        Receiver = new PyroReceiver();
        Devices.Add(DevicePrefab);
        DevicePrefab.SetActive(false);
    }

    private void Update()
    {
        if (!Receiver.Listener.IsListening)
        {
            return;
        }

        if (!IsActive)
        {
            return;
        }
        
        var connected = Receiver.Devices.Values.ToArray();
        for (int i = 0; i < connected.Length; i++)
        {
            var device = connected[i];
            if (Devices.Count == i)
            {
                AddDevice(connected[i], i);
            }
            else
            {
                var deviceObj = Devices[i];
                var comps = deviceObj.GetComponentsInChildren<TextMeshProUGUI>();
                comps[0].text = device.Name;
                comps[1].text = "Sent: " + (device.Sent / 1_000_000d).Round() + "MB";
                comps[2].text = "Received: " + (device.Received / 1_000_000d).Round() + "MB";
                comps[3].text = "Requests: " + device.Requests;
                deviceObj.SetActive(true);
            }
        }
    }

    public void AddDevice(Device device, int index)
    {
        var gb = Instantiate(DevicePrefab, DevicePrefab.transform.parent);
        var position = gb.transform.position;
        position = DevicePrefab.transform.position;
        position -= new Vector3(0, (100 * index), 0);
        gb.transform.position = position;
        Devices.Add(gb);
    }

    private UnityAction StartServer()
    {
        return () =>
        {
            Receiver.Start();
            StatusText.text = "Online";
        };
    }
}