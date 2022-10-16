using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public static class ViewHandler
    {
        public static List<View> Views = new List<View>();
        public static bool Active = false;
        public static void Add(View view)
        {
            Views.Add(view);
        }

        public static void Show(string id)
        {
            Views.First(x => x.Id == id).Show();
        }

        public static void ShowOne(string id)
        {
            foreach (var view in Views)
            {
                if (view.Id == id)
                {
                    view.Show();
                }
                else
                {
                    if (view.IsActive && !view.IsPersistent)
                    {
                        view.Hide();
                    }
                }
            }
        }
        
        public static void Hide(string id)
        {
            Views.First(x => x.Id == id).Hide();
        }

        public static void HideOne(string id)
        {
            foreach (var view in Views)
            {
                if (view.Id == id)
                {
                    view.Hide();
                }
            } 
        }
    }
}