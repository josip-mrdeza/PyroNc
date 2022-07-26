using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc
{
    [RequireComponent(typeof(Text))]
    public class VirtualConsole : MonoBehaviour, IListener
    {
        public Text text;
        public static VirtualConsole Instance;
        private void Awake()
        {
            Instance = this;
            text ??= GetComponent<Text>();
        }

        public static void WriteLine(string message)
        {
            Instance.text.text += message + '\n';
        }

        public async Task OnCommandExecute(Command command) => WriteLine($"{command.Id}: '{command.Args}'.");
    }
}