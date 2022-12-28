using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class ListViewItem : InitializerRoot
    {
        public string Content
        {
            get => TextMesh.text;
            set => TextMesh.text = value;
        }

        public Button Button;
        public TextMeshProUGUI TextMesh;
        public Image Panel;
        public TextMeshProUGUI TypeName;
        public TextMeshProUGUI LengthName;
        public TextMeshProUGUI DateName;
        public Toggle Enabled;
        public bool HasPanel = true;
        public override void Initialize()
        {
            Button = gameObject.GetComponent<Button>();
            var colors = Button.colors;
            colors.pressedColor = Color.clear;
            colors.selectedColor = Color.clear;
            colors.highlightedColor = new Color(125 / 255f, 125 / 255f, 125 / 255f, 1);
            Button.colors = colors;
            TextMesh = gameObject.GetComponent<TextMeshProUGUI>();
            var typeGo = new GameObject("T");
            var lengthGo = new GameObject("L");
            var dateGo = new GameObject("D");
            var enabledGo = new GameObject("E");
            typeGo.transform.SetParent(transform);
            lengthGo.transform.SetParent(transform);
            dateGo.transform.SetParent(transform);
            enabledGo.transform.SetParent(transform);
            TypeName = typeGo.AddComponent<TextMeshProUGUI>();
            LengthName = lengthGo.AddComponent<TextMeshProUGUI>();
            DateName = dateGo.AddComponent<TextMeshProUGUI>();
            Enabled = enabledGo.AddComponent<Toggle>();
            TypeName.alignment = TextAlignmentOptions.Center;
            LengthName.alignment = TextAlignmentOptions.Center;
            DateName.alignment = TextAlignmentOptions.Center;
            typeGo.transform.position = TextMesh.transform.position + new Vector3(480, 0, 0);
            lengthGo.transform.position = typeGo.transform.position + new Vector3(200, 0, 0);
            dateGo.transform.position = lengthGo.transform.position + new Vector3(300, 0, 0);
            enabledGo.transform.position = dateGo.transform.position + new Vector3(500, 0, 0);
            (dateGo.transform as RectTransform).sizeDelta = new Vector2(400, 50);
            if (HasPanel)
            {
                var go = new GameObject("Panel", typeof(Image));
                go.transform.SetParent(transform);
                var tr = go.transform as RectTransform;
                var baseRect = (transform as RectTransform);
                //Debug.Log($"X:{baseRect.x} / Y:{baseRect.y} / W:{baseRect.width} / H:{baseRect.height}");
                tr.localPosition = new Vector3(0, 0);
                tr.sizeDelta = baseRect.sizeDelta;
                Panel = go.GetComponent<Image>();
                var color = Panel.color;
                color.a = 0.05f;
                color.r = 255 / 255f;
                color.g = 255 / 255f;
                color.b = 255 / 255f;
                Panel.color = color;
            }
        }
    }
}