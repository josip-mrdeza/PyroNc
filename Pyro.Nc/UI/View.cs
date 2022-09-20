using System;
using System.Collections.Generic;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class View : MonoBehaviour
    {
        public string Id;
        public List<GameObject> Objects;

        public void Start()
        {
            Globals.Console.PushText($"Begun Start() in View -> Id: {Id}");
            if (string.IsNullOrEmpty(Id))
            {
                Id = gameObject.name;
            }
            Objects = new List<GameObject>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Objects.Add(gameObject.transform.GetChild(i).gameObject);
            }
            Globals.Console.PushText($"Ended Start() in View -> Id: {Id};\n   View has {Objects.Count} items!");
            ViewHandler.Add(this);
        }

        public void Show()
        {
            foreach (var go in Objects)
            {
                go.SetActive(true);
                Globals.Console.PushText($"Showed GameObject: {go.name}!");
            }
        }

        public void Hide()
        {
            foreach (var go in Objects)
            {
                go.SetActive(false);
                Globals.Console.PushText($"Hid GameObject: {go.name}!");
            }
        }
        
    }
}