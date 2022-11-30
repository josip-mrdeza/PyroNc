using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parser;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.Programs;
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
        private PointerEventData _data;
        private bool HasSaved;
        private string fileName;
        private Vector2 LeftTop;
        private int len;

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
            //PopupHandler.PopInputOption("Name your program:", "Ok", Option);
        }

        private void Option(PopupHandler ph)
        {
            if (HasSaved)
            {
                return;
            }

            var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
            local.ModifyFile(ph.Text, Text.text);
            fileName = ph.Text;
            HasSaved = true;
            Globals.Loader.Load();
            Globals.Loader.ShowOnScreen();
        }

        private async void Call()
        {
            Globals.Comment.PushComment("3DView", Color.gray);
            ViewHandler.ShowOne("3DView");

            //var currentCommand = Globals.Tool.Values.Current;
            if (Globals.Tool.Values.IsPaused)
            {
                await Globals.Tool.Resume();
                return;
            }
            CommandHelper.PreviousModal = null;
            var variables = Text.text.ToUpper(CultureInfo.InvariantCulture)
                                .Split('\n')
                                .Select(x => x.Trim().ToUpper()
                                              .FindVariables());
            var commands = variables.Select(x => x.CollectCommands())
                                    .SelectMany(y => y);

            var arr = commands.ToArray();

            foreach (var command in arr)
            {
                if (Globals.Tool.Values.IsReset)
                {
                    return;
                }
                await Globals.Tool.UseCommand(command, true);
                if (command is M02 or M30)
                {
                    return;
                }
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
                    Globals.Localisation.Find(Localisation.MapKey.GCodeNoCommandsFound)
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
        public IEnumerable<string> GetSuggestions(string text)
        {
            List<string[]> variables = null;
            List<ICommand> commands = new List<ICommand>();
            try
            {
                var lines = text.Split('\n').Take(GetLineNumber() + 1).ToArray();
                CommandHelper.PreviousModal = null;
                foreach (var line in lines)
                {
                    variables = line.Trim().FindVariables();
                    commands = variables.CollectCommands();
                }
                return commands.Select(x => $"{x.GetType().Name}: \"{x.Description}\", " +
                                           $"({x.Parameters.Values.Values.Count(v => !float.IsNaN(v)).ToString()}) {x.AdditionalInfo}");
            }
            catch (Exception e)
            {
                 Push(Globals.Localisation.Find(Localisation.MapKey.GCodeFault, 
                                                GetLineAtCaret()),
                      Globals.Localisation.Find(Localisation.MapKey.GenericError, 
                                                e));
                 return new string[]{Globals.Localisation.Find(Localisation.MapKey.GCodeFaultOrUndeclared)
                     + e.Message};
            }
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
                if (string.IsNullOrEmpty(fileName))
                {
                    PopupHandler.PopInputOption(Globals.Localisation.Find(Localisation.MapKey.GCodeNameProgram), 
                                                "Ok", Option);
                }
                else
                {
                    var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
                    local.ModifyFile(fileName, Text.text);
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
            var lines = Text.text.Split('\n');
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
            fileName = id;
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