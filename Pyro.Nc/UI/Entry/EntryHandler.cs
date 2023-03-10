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

public class EntryHandler : LoadingScreenView
{
    public bool IsWaiting { get; set; }
    public static PUser User { get; private set; }
    public static Session Session { get; private set; }
    public static ThreadTaskQueue TaskQueue { get; private set; }
    private static string URL = "https://pyronetserver.azurewebsites.net";
    public override void Initialize()
    {
        //redundant
    }

    public override void FixedUpdate()
    {
        if (IsWaiting)
        {
            base.FixedUpdate();
        }
    }


    private async void Start()
    {
        SetText("Prompting user for login info...");
        IsWaiting = true;
        if (!LocalRoaming.OpenOrCreate("PyroNc\\LoginInfo").Exists("login.json"))
        {
            PopupHandler.PopDualInputOption("Welcome to Pyro Nc", "Register / Log in", "Username", "Password", async ph =>
            {
                bool isFinished = false;
                while (!isFinished)
                {
                    try
                    {
                        SetText("Querying login info from server using data provided...");
                        IsWaiting = true;
                        SetText("Logging in...");
                        await Login(ph);
                        IsWaiting = false;
                        SetText("Done!");
                        SceneManager.LoadSceneAsync("MainScene");
                        isFinished = true;
                    }
                    catch (Exception e)
                    {
                        Push(e.Message);
                        IsWaiting = true;
                        SetText("An error has occured!");
                        await Task.Delay(200);
                        SetText("Retrying...");
                    }
                }
                IsWaiting = false;
            });
        }
        else
        {
            SetText("Reading login info...");
            IsWaiting = true;
            await Login(null);
            IsWaiting = false;
            SetText("Done!");
            SceneManager.LoadSceneAsync("MainScene");
        }
    }

    public async Task Login(PopupHandler ph)
    {
        var lr = LocalRoaming.OpenOrCreate("PyroNc\\LoginInfo");
        var user = lr.ReadFileAs<PUser>("login.json");
        if (user is null)
        {
            SetText("Registering user...");
            user = Register(ph);
            SetText("Logging in...");
            await Login(ph);
            SetText("Done!");
        }
        else
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password))
            {
                UnityEngine.Debug.Log("User login was empty!");
                return;
            }
        }
        IsWaiting = true;
        SetText("Registering for file service...");
        for (int i = 0;; i++)
        {
            try
            {
                var ok = await NetHelpers.PostWithDetails($"{URL}/files/register?userName={user.Name}&password={user.Password}");
                if (ok.IsSuccessStatusCode)
                {
                    break;
                }
                SetText($"[{(int) ok.StatusCode}]: " + ok.ReasonPhrase);
                await Task.Delay(200);
                SetText("Registering for file service...");
            }
            catch (Exception e)
            {
                SetText($"Error: {e.Message}");
            }
        }
        SetText("Done!");
        IsWaiting = true;
        SetText("Requesting session creation...");
        Session s = await NetHelpers.GetJson<Session>($"{URL}/session/random?userName={user.Name}");
        SetText("Done!");
        User = user;
        Session = s;
        SetText("Activating session...");
        ActivateSession(user, s);
        IsWaiting = false;
        SetText("Done!");
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