using System;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class ViewKeyPressHandler : MonoBehaviour
    {
        private void Update()
        {
            foreach (var view in ViewHandler.Views.Values)
            {
                if (Input.GetKeyDown(view.Key))
                {
                    if (view.IsActive)
                    {
                        ViewHandler.HideOne(view.Id);
                    }
                    else
                    {
                        ViewHandler.ShowOne(view.Id);
                    }
                }
            }
        }
    }
}