using System;
using System.Collections.Generic;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI
{
    public class View : InitializerRoot
    {
        public string Id;
        public bool ActiveByDefault;
        public bool IsPersistent;
        public bool IsActive;
        public bool IsDisabled;
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
            RefreshChildObjects();
            ViewHandler.Add(this);
            for (var i = 0; i < Buttons?.Count; i++)
            {
                var button = Buttons?[i];
                var index = i;
                button.onClick.AddListener(() => ShowAtIndex(index));
            }

            if (!ActiveByDefault)
            {
                Hide(); 
            }
            //Push($"Initialized View: {gameObject.name}", $"View has {Objects.Count} children.");
        }

        public void RefreshChildObjects()
        {
            Objects = new List<GameObject>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Objects.Add(gameObject.transform.GetChild(i).gameObject);
            }
        }

        public void Update()
        {
            if (IsActive)
            {
                UpdateView();
            }
        }

        public virtual void UpdateView()
        {
        }

        public void ShowAtIndex(int index)
        {
            ViewHandler.Show(LinkedViews[index].Id);
        }
        public virtual void Show()
        {
            if (IsDisabled)
            {
                return;
            }
            foreach (var go in Objects)
            {
                go.SetActive(true);
            }
            IsActive = true;
            ViewHandler.Active = true;
        }

        public virtual void Hide()
        {
            foreach (var go in Objects)
            {
                go.SetActive(false);
            }
            IsActive = false;
            ViewHandler.Active = false;
            OnHidden?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnHidden;
    }
}