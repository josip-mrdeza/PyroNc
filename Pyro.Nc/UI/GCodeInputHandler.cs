using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parser;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Parsing.SyntacticalCommands;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.Programs;
using Pyro.Nc.UI.UI_Screen;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
            CloseButton.Instance.Show();
            Focus();
        }

        public override void Hide()
        {
            base.Hide();
            ViewHandler.Active = false;
            CloseButton.Instance.Hide();
        }

        public void Focus()
        {
            EventSystem.current.SetSelectedGameObject(Text.gameObject);
            Text.OnPointerClick(_data);
        }

        public void MarkDirty()
        {
            HasSaved = false;
        }

        public override void Initialize()
        {
            //Text.onValueChanged.AddListener(_ => ApplySuggestions());
            Button.onClick.AddListener(async () => await Call(Text.text, true));
            _data = new PointerEventData(EventSystem.current);
            Globals.GCodeInputHandler = this;
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

        public async Task Call(string text, bool reset)
        {
            //var currentCommand = Globals.Tool.Values.Current;
            if (Globals.Tool.Values.IsPaused)
            {
                await Globals.Tool.Resume();
                PushComment("Resumed program", Color.gray);
                return;
            }

            if (reset)
            {
                ResetButton.Instance.onClick.Invoke();
                Globals.Comment.PushComment("3DView", Color.gray);
                ViewHandler.ShowOne("3DView");
                CommandHelper.PreviousModal = null;
            }
            var variables = text.ToUpper(CultureInfo.InvariantCulture)
                                .Split('\n')//lines
                                .Select(x => x.Trim().ToUpper().FindVariables()); //line commands
            var commands = variables.Select(x => x.CollectCommands())
                                    .SelectMany(y => y);
            
            var arr = commands.ToList();
            Globals.Tool.Values.IsReset = false;
            for (var i = 0; i < arr.Count; i++)
            {
                var command = arr[i];
                if (Globals.Tool.Values.IsReset)
                {
                    return;
                }

                if (command is ForLoopGCode loop)
                {
                    if (arr.FindLastIndex(x => typeof(EndForGCode) == x.GetType()) == -1)
                    {
                        throw new ForLoopNoEndException();  
                    }
                    for (int j = i+1; j < arr.Count; j++)
                    {
                        var cmnd = arr[j];
                        if (cmnd is EndForGCode)
                        {
                            break;
                        }

                        arr.Remove(cmnd);
                        loop.ContainedCommands.Add((BaseCommand) cmnd);
                        j--;
                    }

                    await command.ExecuteFinal(true, true);
                    continue;
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
                var nPos = (Vector2)localPos.Value - LeftTop;
                Display.transform.localPosition = nPos;
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
            try
            {
                var lines = text.Split('\n').Take(GetLineNumber() + 1).ToArray();
                CommandHelper.PreviousModal = null;
                List<ICommand> commands = null;
                if (lines.Length == 0)
                {
                    return Enumerable.Empty<string>();
                }
                foreach (var line in lines)
                {
                    var variables = line.Trim().ToUpper().FindVariables();
                    commands = variables.CollectCommands();
                }
                return commands!.Select(x => $"{x.GetType().Name}: \"{x.Description}\", " +
                                           $"({x.Parameters?.Values.Values.Count(v => !float.IsNaN(v)).ToString()}) {x.AdditionalInfo}");
            }
            catch (Exception e)
            {
                 Push(Globals.Localisation.Find(Localisation.MapKey.GCodeFault, 
                                                GetLineAtCaret()), Globals.Localisation.Find(Localisation.MapKey.GenericError, 
                                                e)
                      );
                 return new []{Globals.Localisation.Find(Localisation.MapKey.GCodeFaultOrUndeclared)
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

        public override void UpdateView()
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
            
            ApplySuggestions();
            UpdateDisplaySize(len);
            var infos = Text.textComponent.textInfo.wordInfo;
            for (int i = 0; i < infos.Length; i++)
            {
                try
                {
                    var word = infos[i];
                    var str = word.GetWord();
                    var isParameter = Regex.IsMatch(str, @"[xXyYzZiIjJ](\d+)|(\.\d+)");
                    if (isParameter)
                    {
                        SetCharacterColors(word.firstCharacterIndex, word.lastCharacterIndex + 1, new Color32(177, 3, 252, 200));
                        continue; 
                    }
                    var isGCommand = Regex.IsMatch(str, @"(G|g)\d+");
                    if (isGCommand)
                    {
                        SetCharacterColors(word.firstCharacterIndex, word.lastCharacterIndex + 1, new Color32(52, 235, 152, 200));
                        continue;
                    }
                    var isMCommand = Regex.IsMatch(str, @"(M|m)\d+");
                    if (isMCommand)
                    {
                        SetCharacterColors(word.firstCharacterIndex, word.lastCharacterIndex + 1, new Color32(255, 255, 0, 200));
                        continue;
                    }

                    var isArbCommand = Regex.IsMatch(str, @"[^xXyYzZiIjJ]\d*");
                    if (isArbCommand)
                    {
                        SetCharacterColors(word.firstCharacterIndex, word.lastCharacterIndex + 1, new Color32(50, 120, 200, 200));
                        continue;
                    }

                    SetCharacterColors(word.firstCharacterIndex, word.lastCharacterIndex + 1, new Color32(255, 0, 0, 200));
                }
                catch
                {
                    //ignore
                }
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
                var charInfo = textInfo.characterInfo[Text.caretPosition];
                var charPos = charInfo.bottomRight;
                if (charInfo.baseLine < -540)
                {
                    var lineIndex = charInfo.lineNumber;
                    var multiplier = lineIndex / 20;
                    charInfo.baseLine += 500 * multiplier;
                }
                charPos.y = charInfo.baseLine;
                return charPos;
            }

            return null;
        }

        public void SetCharacterColors(int startIndex, int endIndex, Color32 color)
        {
            var txt = Text.textComponent.textInfo;
            var characters = txt.characterInfo;
            for (int i = startIndex; i < endIndex; i++)
            {
                var index = characters[i].vertexIndex;
                for (int j = 0; j < 4; j++)
                {
                    txt.meshInfo[characters[i].materialReferenceIndex].colors32[index + j] = color;
                }
            }
            txt.meshInfo[0].mesh.vertices = txt.meshInfo[0].vertices;
            Text.textComponent.UpdateVertexData();
        }
    }
}