using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parser;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pyro.Nc.UI
{
    public class GCodeInputHandler : View
    {
        [SerializeField] 
        private TMP_InputField Text;
        [SerializeField]
        private GameObject Display;
        public TextMeshProUGUI LineNumber;
        public TextMeshProUGUI SuggestionDisplay;
        public Button Button;
        public PopupHandler Handler;
        private PointerEventData _data;
        private bool HasSaved;
        private string fileName;

        public override void Show()
        {
            base.Show();
            ViewHandler.Active = true;
            Focus();
        }

        public override void Hide()
        {
            base.Hide();
            ViewHandler.Active = false;
        }

        public void Focus()
        {
            EventSystem.current.SetSelectedGameObject(Text.gameObject);
            Text.OnPointerClick(_data);
        }

        public override void Initialize()
        {
            //Text.onValueChanged.AddListener(_ => ApplySuggestions());
            Button.onClick.AddListener(Call);
            _data = new PointerEventData(EventSystem.current);
            base.Initialize();
            Handler = GetComponent<PopupHandler>();
            Handler.Initialize();
            Handler.PrefabButtons[0].onClick.AddListener(() =>
            {
                if (HasSaved)
                {
                    return;
                }
                var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
                local.ModifyFile(Handler.Text, Text.text);
                HasSaved = true;
            });
        }

        private async void Call()
        {
            Globals.Comment.PushComment("3DView", Color.gray);
            ViewHandler.ShowOne("3DView");

            //var currentCommand = Globals.Tool.Values.Current;
            var variables = Text.text.ToUpper(CultureInfo.InvariantCulture)
                                .Split('\n')
                                .Select(x => x.Trim().ToUpper()
                                              .FindVariables());
            var commands = variables.Select(x => x.CollectCommands())
                                    .SelectMany(y => y);

            var arr = commands.ToArray();
            if (arr.FirstOrDefault() is M03 or M04)
            {
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    await Globals.Tool.UseCommand(arr[i], true);
                }

                return;
            }

            foreach (var command in arr)
            {
                await Globals.Tool.UseCommand(command, true);
            }
        }

        public void ApplySuggestions()
        {
            HasSaved = false;
            //var snip = Snip(Text.caretPosition);
            // if (string.IsNullOrEmpty(snip))
            // {
            //     return;
            // }
            var suggestions = GetSuggestions(Text.text).ToArray();
            if (suggestions.Length == 0)
            {
                suggestions = new string[]
                {   
                    "No commands found!"
                };
            }
            SuggestionDisplay.text = string.Join("\n", suggestions);
            UpdateDisplayPosition();
            UpdateDisplaySize(suggestions.Sum(x=> x.Length) / 30);
        }

        private void UpdateDisplayPosition()
        {
            var localPos = GetLocalCaretPosition();
            if (localPos != null)
            {
                Display.transform.localPosition = ((Vector2) localPos.Value) - LeftTop;
            }
        }

        private void UpdateDisplaySize(int length)
        {
            len = length > 0 ? length : 1;
            var rtr1 = (Display.transform as RectTransform);
            var rtr2 = SuggestionDisplay.transform as RectTransform;
            var size = rtr1.sizeDelta = new Vector2(500, 50 * length * 1.25f);
            rtr2.sizeDelta = rtr1.sizeDelta;
            LeftTop = new Vector2(-size.x / 2, size.y / 2);
        }

        private Vector2 LeftTop;
        private int len;
        public IEnumerable<string> GetSuggestions(string text)
        {
            List<string[]> variables = null;
            List<ICommand> commands = new List<ICommand>();
            try
            {
                var lines = text.SplitNoAlloc('\n').Take(GetLineNumber() + 1).ToArray();
                foreach (var line in lines)
                {
                    variables = line.Trim().FindVariables();
                    commands = variables.CollectCommands();
                }
                return commands.Select(x => $"{x.GetType().Name}: \"{x.Description}\" {x.AdditionalInfo}");
            }
            catch (Exception e)
            {
                 Push($"Error in GetSuggestions: line -> \"{GetLineAtCaret()}\"", $"Error: {e}");
                 return new string[]{"Faulty variable / Undeclared command! - " + e.Message};
            }

            return new string[]{"Faulty variable / Undeclared command!"};
        }

        public string Snip(int caretPos)
        {
            var result = GetLineAtCaret();
            if (result != null)
            {
                return result;
            }
            throw new Exception($"GCode Snip exception, no line matched the caret's position. Caret pos: {caretPos}, Text len: {Text.text.Length}; sum = {0}");
        }

        private void LateUpdate()
        {
            var lineNum = GetLineNumber();
            LineNumber.text = $"Line: {lineNum.ToString()} | {Text.caretPosition.ToString()}";
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.S))
            {
                //TODO prompt text for file name.
                if (string.IsNullOrEmpty(Handler.Text))
                {
                    Handler.Pop("What would you like to name this program?");
                }
                else
                {
                    var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
                    local.ModifyFile(Handler.Text, Text.text);
                    HasSaved = true;
                }
            }

            if (IsActive)
            {
                ApplySuggestions();
                UpdateDisplaySize(len);
            }
        }

        public int GetLineNumber()
        {
            var lines = Text.text.SplitNoAlloc('\n').ToArray();
            var cNum = Text.caretPosition;
            var sum = 0;
            int i;
            for (i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length == 0)
                {
                    sum += 1;
                }
                else
                {
                    sum += line.Length + 1;
                }
                if (cNum < sum)
                {
                    return i;
                }
            }

            return 0;
        }
        
        public string GetLineAtCaret()
        {
            return Text.text.Split('\n')[GetLineNumber()];
        }

        public void LoadText(string text, string id)
        {
            Text.text = text;
            Handler.Text = id;
        }
        
        public Vector3? GetLocalCaretPosition()
        {
            if (Text.isFocused)
            {
                var textInfo = Text.textComponent.textInfo;
                var charPos = textInfo.characterInfo[Text.caretPosition].bottomRight;

                return charPos;
            }

            return null;
        }
    }
}