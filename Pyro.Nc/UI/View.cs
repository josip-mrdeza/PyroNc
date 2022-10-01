using System;
using System.Collections.Generic;
using Pyro.Nc.Configuration;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI
{
    public class View : InitializerRoot
    {
        public string Id;
        public bool ActiveByDefault;
        public bool IsActive;
        public KeyCode Key;
        public List<GameObject> Objects;
        public List<Button> Buttons;
        public List<View> LinkedViews;

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = gameObject.name;
            }
            Objects = new List<GameObject>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Objects.Add(gameObject.transform.GetChild(i).gameObject);
            }
            ViewHandler.Add(this);
            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];
                var index = i;
                button.onClick.AddListener(() => ShowAtIndex(index));
            }

            if (!ActiveByDefault)
            {
                Hide(); 
            }
            PyroConsoleView.PushTextStatic($"Initialized View: {gameObject.name}", $"View has {Objects.Count} children.");
        }

        public void ShowAtIndex(int index)
        {
            ViewHandler.Show(LinkedViews[index].Id);
        }
        public void Show()
        {
            foreach (var go in Objects)
            {
                go.SetActive(true);
            }
            IsActive = true;
        }

        public void Hide()
        {
            foreach (var go in Objects)
            {
                go.SetActive(false);
            }
            IsActive = false;
        }
    }
}