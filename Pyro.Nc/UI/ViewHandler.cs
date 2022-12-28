using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public static class ViewHandler
    {
        public static readonly Dictionary<string, View> Views = new Dictionary<string, View>();
        public static bool Active = false;
        public static void Add(View view)
        {
            Views.Add(view.Id, view);
        }

        public static void Show(string id)
        {
            Views[id].Show();
        }

        public static void ShowOne(string id)
        {
            View v = null;
            foreach (var view in Views.Values)
            {
                if (view.Id == id)
                {
                    v = view;
                }
                else
                {
                    if (view.IsActive && !view.IsPersistent)
                    {
                        view.Hide();
                    }
                }
            }
            v.Show();
        }
        
        public static void Hide(string id)
        {
            Views[id].Hide();
        }

        public static void HideOne(string id)
        {
            Views[id].Hide();
        }
    }
}