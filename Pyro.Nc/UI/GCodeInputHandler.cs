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
        public bool Loop;

        private void Start()
        {
            Text = GetComponent<TMP_InputField>();
            ApplySuggestions();
        }

        public async void ApplySuggestions()
        {
            while (Loop)
            {
                var suggestions = GetSuggestions().ToArray();
                SuggestionDisplay.text = "Suggestions:\n";
                SuggestionDisplay.text = string.Join("\n", suggestions);
                await Task.Delay(1);
                await Task.Yield();
            }
        }

        public IEnumerable<string> GetSuggestions()
        {
            var variables = Text.text.FindVariables();
            var commands = variables.CollectCommands();

            return commands.Select(x => $"{x.GetType().Name}, {x.Description}");
        }
    }
}