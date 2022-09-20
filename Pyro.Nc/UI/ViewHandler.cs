using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public static class ViewHandler
    {
        public static List<View> Views = new List<View>();

        public static void Add(View view)
        {
            Views.Add(view);
        }

        public static void Show(string id)
        {
            Views.First(x => x.Id == id).Show();
        }
        
        public static void Hide(string id)
        {
            Views.First(x => x.Id == id).Hide();
        }
    }
}