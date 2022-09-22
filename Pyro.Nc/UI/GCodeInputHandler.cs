using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Parsing;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class GCodeInputHandler : MonoBehaviour
    {
        public TMP_InputField Text;
        public TextMeshProUGUI SuggestionDisplay;
        private void Start()
        {
            Text.onValueChanged.AddListener(_ => ApplySuggestions());
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
            var lines = Text.text.Split('\n');
            var count = Text.text.Count(x => x == '\n');
            var sum = count;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                sum += line.Length;
                if (caretPos <= sum)
                {
                    return line;
                }
            }

            throw new Exception($"GCode Snip exception, no line matched the caret's position. Caret pos: {caretPos}, Text len: {Text.text.Length}; sum = {sum}");
        }
    }
}