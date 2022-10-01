using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI
{
    public class GCodeInputHandler : View
    {
        public TMP_InputField Text;
        public TextMeshProUGUI LineNumber;
        public TextMeshProUGUI SuggestionDisplay;
        public Button Button;
        public override void Initialize()
        {
            Text.onValueChanged.AddListener(_ => ApplySuggestions());
            Button.onClick.AddListener(async () =>
            {
                var variables = Text.text.Split('\n').Select(x => x.Trim().FindVariables());
                var commands = variables.Select(x => x.CollectCommands()).SelectMany(y => y);

                var arr = commands.ToArray();

                foreach (var command in arr)
                {
                    await Globals.Tool.UseCommand(command, true);
                }
            });
            base.Initialize();
        }

        public void ApplySuggestions()
        {
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
            var variables = line.Trim().FindVariables();
            var commands = variables.CollectCommands();

            return commands.Select(x => $"{x.GetType().Name}, {x.Description}");
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
    }
}