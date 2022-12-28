using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class ListView : InitializerRoot
    {
        private static readonly Type[] Components = new[]
        {
            typeof(ListViewItem),
            typeof(TextMeshProUGUI),
            typeof(Button)
        };
        public List<ListViewItem> list;
        public List<object> contents;
        public RectTransform TemplateTransform;
        public override void Initialize()
        {
            list = new List<ListViewItem>();
            contents = new List<object>();
        }

        public virtual void Refresh(List<object> objs)
        {
            contents = objs;
            var position = TemplateTransform.position;
            float height = position.y;
            float start = position.x;
            for (int i = 0; i < contents.Count; i++, height -= 55)
            {
                ListViewItem o;
                if (i + 1 > list.Count)
                {
                    o = new GameObject(i.ToString(), Components)
                        .GetComponent<ListViewItem>();
                    RectTransform transform1 = o.transform as RectTransform;
                    transform1.SetParent(this.transform);
                    transform1.position = new Vector3(start, height);
                    transform1.sizeDelta = new Vector2(500, 50);
                    o.Initialize();
                    o.Content = objs[i].ToString();
                    var fileInfo = LocalRoaming.OpenOrCreate("PyroNc\\GCode").Files[o.Content];
                    o.TypeName.text = fileInfo.Extension.ToUpper();
                    o.LengthName.text = File.ReadLines(fileInfo.FullName).Count().ToString();
                    o.DateName.text = fileInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture);
                    o.Button.onClick.AddListener(() => Handler(o));
                    o.Enabled.onValueChanged.AddListener(b =>
                    {
                        fileInfo.Attributes = b ? FileAttributes.Normal : FileAttributes.Hidden;
                    });
                    list.Add(o);
                    continue;
                }
                o = list[i];
                o.Content = objs[i].ToString();
                o.transform.position = new Vector3(start, height);
                //o.Initialize();
            }
        }

        public virtual void Handler(ListViewItem item)
        {
            var id = "GCodeEditor";
            var view = ViewHandler.Views[id] as GCodeInputHandler;
            ViewHandler.ShowOne(view.Id);
            view.LoadText(LocalRoaming.OpenOrCreate("PyroNc\\GCode").ReadFileAsText(item.Content), item.Content);
        }

        public virtual void RefreshSingular(int index, object obj)
        {
            list[index].Content = obj.ToString();
        }
    }
}