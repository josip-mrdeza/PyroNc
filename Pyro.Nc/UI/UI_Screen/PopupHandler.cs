using System;
using System.Threading.Tasks;
using Pyro.Nc.Configuration.Startup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class PopupHandler : InitializerRoot
    {
        private GameObject ActivePrefab;
        public GameObject Prefab;
        public Canvas Canvas;
        public string PromptMessage;
        public TextMeshProUGUI PromptText;
        public Button[] PrefabButtons;
        public TMP_InputField PrefabInput;
        public string Text
        {
            get => PrefabInput.text;
            set => PrefabInput.text = value;
        }

        public void Pop(string msg)
        {
            PromptText.text = msg;
            Open();
        }

        public override void Initialize()
        {
            var go = ActivePrefab = Instantiate(Prefab, Canvas.transform);
            go.transform.localPosition = Vector3.zero;
            PromptText = go.GetComponentInChildren<TextMeshProUGUI>();
            PrefabInput = go.GetComponentInChildren<TMP_InputField>();
            PrefabButtons = go.GetComponentsInChildren<Button>();
            AddListeners();
            Close();
        }

        public virtual void AddListeners()
        {
            if (PrefabButtons is not null)
            {
                if (PrefabButtons.Length == 2)
                {
                    PrefabButtons[1].onClick.AddListener(Close);
                }
                else if (PrefabButtons.Length == 1)
                {
                    PrefabButtons[0].onClick.AddListener(Close);
                }
            }
        }
        public void Open()
        {
            if (!ActivePrefab.activeSelf)
            {
                ActivePrefab.SetActive(true);
                ViewHandler.Active = true;
            }
        }
        
        public virtual void Close()
        {
            if (ActivePrefab.activeSelf)
            {
                ActivePrefab.SetActive(false);
                ViewHandler.Active = false;
            }
        }
    }
}