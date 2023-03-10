using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.UI.Entry;
using Pyro.Nc.UI.Programs;
using Pyro.Nc.UI.UI_Screen;
using Pyro.Net;
using TMPro;
using Unity.Collections;
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
        public Button Simulation2DButton;
        public GCodePainter Painter;
        public int Line;
        private const string Address = "https://pyronetserver.azurewebsites.net/files";
        private PointerEventData _data;
        private bool HasSaved;
        private bool IsFileLocal = true;
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
            Text.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        public void SelectText(int lineIndex)
        {
            var lines = Text.textComponent.textInfo.lineInfo;
            var line = lines[lineIndex];
            Text.caretPosition = line.firstCharacterIndex;
            Text.selectionAnchorPosition = line.firstCharacterIndex;
            Text.selectionFocusPosition = line.lastVisibleCharacterIndex;
            Text.OnUpdateSelected(new BaseEventData(EventSystem.current));
            Text.ForceLabelUpdate();
        }

        public void MarkDirty()
        {
            HasSaved = false;
        }

        public async Task ApplyChangesToFileServerSide(string fn, string content)
        {
            Push($"[{EntryHandler.User.Name}] - Updating file '{fn}' with a length of '{content.Length}' bytes.");
            await NetHelpers.Post($"https://pyronetserver.azurewebsites.net/files/{fn}?userName={EntryHandler.User.Name}&password={EntryHandler.User.Password}", content);
        }
        public override async Task InitializeAsync()
        {
            InvokeRepeating(nameof(UpdateViewV2), 0, 0.1f);
            Button.onClick.AddListener(async () => await Call(Text.text, true));
            Simulation2DButton.onClick.AddListener(async () => await Call(Text.text, true, true));
            Text.onValueChanged.AddListener(_ =>
            {
                ApplyColors();
            });
            Globals.GCodeInputHandler = this;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
            var user = EntryHandler.User;
            var addr = $"https://pyronetserver.azurewebsites.net";
            var allFiles = await NetHelpers.GetJson<string[]>($"{addr}/files/all?userName={user.Name}&password={user.Password}");
            Push($"Queried a total of '{allFiles.Length}' files for user '{user.Name}'!");
            foreach (var file in allFiles)
            {
                Push($"Reading data for file: '{file}'...");
                var fileData = await NetHelpers.GetData($"{addr}/files/{file}?userName={user.Name}&password={user.Password}");
                Push($"Finished downloading file: '{file}' with a length of '{fileData.Length}' bytes.");
                if (roaming.Exists(file))
                {
                    await roaming.ModifyFileAsync($"{file}", fileData);
                }
            }
        }

        private async Task Option(PopupHandler ph)
        {
            if (HasSaved)
            {
                return;
            }
            
            var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
            var txt = ph.PrefabInputs[0].text;
            if (txt.StartsWith("Server-"))
            {
                await ApplyChangesToFileServerSide(txt, Text.text);
            }
            await local.ModifyFileAsync(txt, Text.text);
            fileName = txt;
            HasSaved = true;
            Globals.Loader.Load();
            Globals.Loader.ShowOnScreen();
        }

        public async Task Call(string text, bool reset, bool is2d = false)
        {
            MachineBase.CurrentMachine.SimControl.ResetSimulation();
            MachineBase.CurrentMachine.StateControl.FreeControl();
            InsertGCodeIntoQueue(text, is2d);
            await MachineBase.CurrentMachine.Runner.ExecuteAll();
            Hide();
        }

        public void InsertGCodeIntoQueue(string text, bool is2d = false)
        {
            var variables = text.ToUpper(CultureInfo.InvariantCulture)
                                .Split('\n')                                      //lines
                                .Select(x => x.Trim().ToUpper().FindVariables()); //line commands
            var cmnds = variables.Select(x => x.CollectCommands());//.SelectMany(y => y);
            var arr = cmnds.ToList();
            var lines = arr;
            if (!lines.Last().Exists(x => x.IsMatch(typeof(M02)) || x.IsMatch(typeof(M30))))
            {
                throw new MissingEndOfProgramException();
            }
            //Globals.Tool.Values.IsReset = false;
            for (int i = 0; i < arr.Count; i++)
            {
                Line = i;
                var line = lines[i];
                for (var index = 0; index < line.Count; index++)
                {
                    var command = line[index];
                    command.Line = i;
                    command.Is2DSimulation = is2d;

                    if (command is ForLoopGCode loop)
                    {
                        if (!lines.Exists(l => l.Exists(c => c.GetType() == typeof(EndForGCode))))
                        {
                            throw new ForLoopNoEndException();
                        }

                        for (int j = i + 1; j < lines.Count; j++)
                        {
                            var lineNested = lines[j];
                            if (lineNested.Exists(x => x.GetType() == typeof(EndForGCode)))
                            {
                                break;
                            }

                            lines.Remove(lineNested);
                            loop.ContainedCommands.AddRange(lineNested);
                            j--;
                        }

                        //await command.ExecuteFinal(true, true);
                        MachineBase.CurrentMachine.Runner.Queue.Enqueue(command);

                        continue;
                    }

                    MachineBase.CurrentMachine.Runner.Queue.Enqueue(command);
                    if (command is M02 or M30)
                    {
                        break;
                    }
                }
            }
        }

        public void InsertGCodeIntoQueue(bool is2d)
        {
            InsertGCodeIntoQueue(Text.text, is2d);
        }

        public void ApplySuggestions()
        {
            HasSaved = false;
            if (MachineBase.CurrentMachine.StateControl.IsFree)
            {
                MachineBase.CurrentMachine.SimControl.SoftResetCodeSimulation();
            }
            var str = Text.text;
            string[] suggestions = null;
            suggestions = GetSuggestions(str).ToArray();
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
            var size = rtr1.sizeDelta = new Vector2(500, 50 * length * 1.5f);
            rtr2.sizeDelta = rtr1.sizeDelta;
            LeftTop = new Vector2(-size.x / 2, size.y / 2);
        }
        public IEnumerable<string> GetSuggestions(string text)
        {
            try
            {
                var lines = text.Split('\n').Take(GetLineNumber() + 1).ToArray();
                CommandHelper.PreviousModal = null;
                List<BaseCommand> commands = null;
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
                 /*Push(Globals.Localisation.Find(Localisation.MapKey.GCodeFault, 
                                                GetLineAtCaret()), Globals.Localisation.Find(Localisation.MapKey.GenericError, 
                                                e.Message)
                      ); */
                 return new []{$"GCODE Error - {e.Message}"};
            }
        }

        public async void UpdateViewV2()
        {
            if (!IsActive)
            {
                return;
            }
            var lineNum = GetLineNumber() + 1;
            LineNumber.text = $"Line: {lineNum.ToString()} | {Text.caretPosition.ToString()}";
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.S))
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    PopupHandler.PopInputOption(Globals.Localisation.Find(Localisation.MapKey.GCodeNameProgram), 
                                                "Ok", async ph => await Option(ph));
                }
                else
                {
                    if (IsFileLocal)
                    {
                        var local = LocalRoaming.OpenOrCreate("PyroNc\\GCode");
                        await local.ModifyFileAsync(fileName, Text.text);
                        if (fileName.StartsWith("Server-"))
                        {
                            await ApplyChangesToFileServerSide(fileName, Text.text);
                        }
                        HasSaved = true;
                    }
                    else
                    {
                        var req = NetHelpers.Post($"{Address}/{fileName}", Text.text);
                        HasSaved = true;
                    }
                }
            }
            
            ApplySuggestions();
            UpdateDisplaySize(len);
        }

        public override void UpdateView()
        {
            //ApplyColors();
        }

        private void ApplyColors()
        {
            if (Text.text.Length > 400)
            {
                return;
            }
            var first = Text.textComponent.firstVisibleCharacter;
            var txt = Text.textComponent.textInfo;
            var last = txt.lineInfo[txt.lineCount-1].lastVisibleCharacterIndex;
            var lines = Text.text.Substring(first, last).Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    var line = lines[i];
                    var words = line.Split(' ');
                    foreach (var word in words)
                    {
                        PaintWords(word, index);
                        index += word.Length + 1;
                    }
                }
                catch (Exception e)
                {
                    Push($"![Painter]: {e.Message}!~");
                    //ignore
                }
            }
        }

        private void PaintWords(string word, int fci)
        {
            var isParameter = RegexPatterns.CompleteParameterCheck.IsMatch(word);
            var lci = fci + word.Length + 1;
            if (isParameter)
            {
                SetCharacterColors(fci, lci,
                                   new Color32(177, 3, 252, 200));

                return;
            }

            var isGCommand = RegexPatterns.CompleteGFunctionCheck.IsMatch(word);
            if (isGCommand)
            {
                SetCharacterColors(fci, lci,
                                   new Color32(52, 235, 152, 200));

                return;
            }

            var isMCommand = RegexPatterns.CompleteMFunctionCheck.IsMatch(word);
            if (isMCommand)
            {
                SetCharacterColors(fci, lci,
                                   new Color32(255, 255, 0, 200));

                return;
            }

            //var isArbCommand = RegexPatterns.CompleteArbitraryFunctionCheck.IsMatch(word);
            if (false)//isArbCommand)
            {
                SetCharacterColors(fci, lci,
                                   new Color32(50, 120, 200, 200));

                return;
            }

            SetCharacterColors(fci, lci, new Color32(255, 0, 0, 200));
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

        public void AddNumerations(int step)
        {
            var str = Text.text;
            StringBuilder builder = new StringBuilder();
            var split = str.Split('\n');
            for (int i = 1; i < split.Length + 1; i++)
            {
                var s = split[i - 1];
                if (!string.IsNullOrEmpty(s))
                {
                    builder.Append("N");
                    builder.Append(i * 10);
                    builder.Append(' ');
                    if (s[0] == 'N')
                    {
                        s = s.Remove(0, s.IndexOf(' '));
                    }
                    builder.AppendLine(s);
                }
            }
            Text.text = builder.ToString();
        }
    }
}