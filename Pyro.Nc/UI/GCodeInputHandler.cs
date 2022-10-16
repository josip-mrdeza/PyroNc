using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Managers;
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
            Text.onValueChanged.AddListener(_ => ApplySuggestions());
            Button.onClick.AddListener(async () =>
            {
                Focus();
                //var currentCommand = Globals.Tool.Values.Current;
                var variables = Text.text.ToUpper(CultureInfo.InvariantCulture)
                                    .Split('\n')
                                    .Select(x => x.Trim()
                                                  .FindVariables());
                var commands = variables
                               .Select(x => x.CollectCommands())
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
            });
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

        public void ApplySuggestions()
        {
            HasSaved = false;
            var snip = Snip(Text.caretPosition);
            if (string.IsNullOrEmpty(snip))
            {
                return;
            }
            var suggestions = GetSuggestions(snip);
            SuggestionDisplay.text = string.Join("\n", suggestions);
        }

        public IEnumerable<string> GetSuggestions(string line)
        {
            List<string[]> variables = null;
            List<ICommand> commands = null;
            try
            {
                variables = line.Trim().FindVariables();
                commands = variables.CollectCommands();
                return commands.Select(x => $"{x.GetType().Name}, {x.Description}");

            }
            catch (Exception e)
            {
                 Push($"Error in GetSuggestions: line -> \"{line}\"");
            }

            return new string[1]{"Faulty variable / Undeclared command!"};
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
        }

        public int GetLineNumber()
        {
            var lines = Text.text.Split('\n');
            var count = Text.text.Count(x => x == '\n');
            var sum = count;
            var caretPos = Text.caretPosition;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                sum += line.Length;
                if (caretPos <= sum)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public string GetLineAtCaret()
        {
            var lines = Text.text.Split('\n');
            var count = Text.text.Count(x => x == '\n');
            var sum = count;
            var caretPos = Text.caretPosition;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                sum += line.Length;
                if (caretPos <= sum)
                {
                    return lines[i];
                }
            }

            return null;
        }

        public void LoadText(string text, string id)
        {
            Text.text = text;
            Handler.Text = id;
        }
    }
}