using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using Pyro.Net;
using PyroNetServerIntermediateLibrary;
using TinyClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.Net;

public class SessionExplorer : View
{
    public GameObject SessionPrefab;
    private List<GameObject> gameObjects;
    private Session[] Sessions;
    private SessionManager Manager;
    public override void Initialize()
    {
        base.Initialize();
        SessionPrefab.SetActive(false);
        gameObjects = new List<GameObject>();
        InvokeRepeating(nameof(UpdateGbs), 0, 0.5f);
        Manager = gameObject.AddComponent<SessionManager>();
    }

    private async void UpdateGbs()
    {
        if (!IsActive)
        {
            return;
        }
        
        Sessions = await NetHelpers.GetJson<Session[]>($"{Manager.Address}session/all");
        var connected = Sessions;
        if (connected.Length < gameObjects.Count)
        {
            foreach (var go in gameObjects)
            {
                go.SetActive(false);
            }
        }
        for (int i = 0; i < connected.Length; i++)
        {
            var session = connected[i];
            if (gameObjects.Count == i)
            {
                AddSession(connected[i], i);
            }
            else
            {
                var sessionObj = gameObjects[i];
                var comps = sessionObj.GetComponentsInChildren<TextMeshProUGUI>();
                var time = session.LastUpdated;
                var ts = time.TotalMinutes < 1d;
                if (ts)
                {
                    comps[2].text = $"Last Active: {time.TotalSeconds.Round(1).ToString(CultureInfo.InvariantCulture)}s";
                }
                else
                {
                    comps[2].text = $"Last Active: {time.Minutes.ToString(CultureInfo.InvariantCulture)}min {time.Seconds.ToString(CultureInfo.InvariantCulture)}s";
                }
                comps[0].text = $"Owner: {session.Owner}";
                comps[1].text = $"Name: {session.Name}";
                comps[3].text = $"Users: {session.Users.Count.ToString()}";
                sessionObj.SetActive(true);
            }
        }
    }
    
    public void AddSession(Session session, int index)
    {
        var gb = Instantiate(SessionPrefab, SessionPrefab.transform.parent);
        var position = gb.transform.position;
        position = SessionPrefab.transform.position;
        position -= new Vector3(0, (100 * index), 0);
        gb.transform.position = position;
        gameObjects.Add(gb);
        gb.GetComponentInChildren<Button>().onClick.AddListener(async () =>
        {
            //TODO
            await NetHelpers.Put($"{Manager.Address}session/update/users?id={session.Name}&password={session.Password}&userName={Device.Current.Name}");
            Globals.Comment.PushComment($"Connected to session: {session.Name} by {session.Owner}!", Color.green);
            ViewHandler.ShowOne("3DView");
        });
        gb.SetActive(true);
    }
}