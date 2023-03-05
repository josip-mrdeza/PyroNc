using System;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Exceptions;
using Pyro.Nc.UI.UI_Screen;
using Pyro.Net;
using Pyro.Threading;
using PyroNetServerIntermediateLibrary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pyro.Nc.UI.Entry;

public class EntryHandler : MonoBehaviour
{
    public static PUser User { get; private set; }
    public static Session Session { get; private set; }
    public static ThreadTaskQueue TaskQueue { get; private set; }
    private static string URL = "https://pyronetserver.azurewebsites.net";

    private async void Start()
    {
        if (!LocalRoaming.OpenOrCreate("PyroNc\\LoginInfo").Exists("login.json"))
        {
            PopupHandler.PopDualInputOption("Welcome to Pyro Nc", "Log in", async ph => await Login(ph));
        }

        await Login(null);
        SceneManager.LoadScene("MainScene");
    }

    public async Task Login(PopupHandler ph)
    {
        var lr = LocalRoaming.OpenOrCreate("PyroNc\\LoginInfo");
        var user = lr.ReadFileAs<PUser>("login.json");
        if (user is null)
        {
            user = Register(ph);
            await Login(ph);
        }
        else
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password))
            {
                UnityEngine.Debug.Log("User login was empty!");
                return;
            }
        }

        Session s = await NetHelpers.GetJson<Session>($"{URL}/session/random?userName={user.Name}");
        User = user;
        Session = s;
        ActivateSession(user, s);
    }

    public PUser Register(PopupHandler ph)
    {
        ph.ButtonTexts[0].text = "Register";
        var name = ph.PrefabInputs[0].text;
        var password = ph.PrefabInputs[1].text;
        var user = new PUser(name, password);
        var lr = LocalRoaming.OpenOrCreate("PyroNc\\LoginInfo");
        lr.AddFile("login.json", user);

        return user;
    }

    public void ActivateSession(PUser user, Session s)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (Session is null)
                    {
                        throw new NotSupportedException("Session cannot be null!");
                    }
                    await NetHelpers.Put($"{URL}/session/update?id={Session.Name}&password={Session.Password}");
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    //NotifyException.CreateNotifySystemException<Exception>(this, e.Message);
                    UnityEngine.Debug.Log(e.Message);
                    Thread.Sleep(5000);
                }
            }
        });
    }
}