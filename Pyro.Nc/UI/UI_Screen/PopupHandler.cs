using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pyro.Nc.UI.UI_Screen
{
    public class PopupHandler : InitializerRoot
    {
        protected GameObject ActivePrefab;
        //public GameObject Prefab;
        public Canvas Canvas;
        public string PromptMessage;
        public TextMeshProUGUI PromptText;
        public Button[] PrefabButtons;
        public TextMeshProUGUI[] ButtonTexts;
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
            Canvas = transform.parent.GetComponent<Canvas>();
            var go = ActivePrefab = InitializeCrucial();
            go.transform.localPosition = Vector3.zero;
            PromptText = go.GetComponentInChildren<TextMeshProUGUI>();
            PrefabInput = go.GetComponentInChildren<TMP_InputField>();
            PrefabButtons = go.GetComponentsInChildren<Button>();
            ButtonTexts = PrefabButtons.Select(x => x.GetComponentInChildren<TextMeshProUGUI>()).ToArray();
            Close();
        }

        public virtual GameObject InitializeCrucial()
        {
            return Instantiate(Resources.Load("Popup_Text", typeof(GameObject)) as GameObject, Canvas.transform);
        }
        
        public virtual void AddListeners(params Action<PopupHandler>[] options)
        {
            RemoveListeners();

            var max = PrefabButtons.Length;
            for (int i = 0; i < options.Length; i++)
            {
                var unityCall = options[i];
                if (i == max)
                {
                    break;
                }
                PrefabButtons[i].onClick.AddListener(() => unityCall(this));
            }
            
            if (PrefabButtons.Length == 2)
            {
                PrefabButtons[0].onClick.AddListener(Close);
                PrefabButtons[1].onClick.AddListener(Close);
            }
            else if (PrefabButtons.Length == 1)
            {
                PrefabButtons[0].onClick.AddListener(Close);
            }
        }

        public void RemoveListeners()
        {
            if (PrefabButtons is null)
            {
                return;
            }
            foreach (var button in PrefabButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        public void Open()
        {
            ViewHandler.Active = true;
            if (!ActivePrefab.activeSelf)
            {
                ActivePrefab.SetActive(true);
            }
        }
        
        public virtual void Close()
        {
            if (ActivePrefab.activeSelf)
            {
                ActivePrefab.SetActive(false);
                //ViewHandler.Active = false;
            }
        }

        public static void PopDoubleOption(string text, string option1Text, string option2Text, params Action<PopupHandler>[] optionFuncs)
        {
            var handler = Globals.DoublePopupHandler;
            handler.ButtonTexts[0].text = option1Text;
            handler.ButtonTexts[1].text = option2Text;
            handler.RemoveListeners();
            handler.AddListeners(optionFuncs);
            handler.Pop(text);
        }
        
        public static void PopInputOption(string text, string optionText, Action<PopupHandler> optionFunc1, bool smallVersion = false)
        {
            var handler = smallVersion ? Globals.InputPopupHandlerSmall : Globals.InputPopupHandlerLarge;
            handler.ButtonTexts[0].text = optionText;
            handler.RemoveListeners();
            handler.AddListeners(optionFunc1);
            handler.Pop(text);
        }
        public static void PopText(string text)
        {
            var handler = Globals.TextPopupHandler;
            handler.PromptMessage = text;
            handler.ButtonTexts[0].text = "Ok";
            handler.RemoveListeners();
            handler.AddListeners();
            handler.Pop(text);
        }
    }
}