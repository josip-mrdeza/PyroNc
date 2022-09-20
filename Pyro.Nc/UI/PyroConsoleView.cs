using System;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands.Exceptions;
using Pyro.Nc.Simulation;
using TMPro;
using TrCore;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class PyroConsoleView : View
    {
        public TextMeshProUGUI TextMesh;
        public StreamWriter Stream;

        private void Awake()
        {
            UpdateGlobals();
            var fileInfo = CreateFileStream();
            PushEventCreation(fileInfo);
        }

        private void Start()
        {
            base.Start();
            PushCreation();
        }

        private FileInfo CreateFileStream()
        {
            var fileInfo = Globals.Variables.AddVariable("pyroLog.txt");
            Stream = fileInfo.CreateText();
            PushText($"Created Log Stream in: {fileInfo.FullName}.");
            return fileInfo;
        }

        private void UpdateGlobals()
        {
            Globals.Console = this;
            Globals.Variables ??= new LocalVariables();
            Globals.Variables.Init("PyroNc");
        }

        private void PushEventCreation(FileInfo fileInfo)
        {
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                PushText($"LogType.{type}:\n    --{condition ?? "Empty"}\n    --StackTrace:{stackTrace ?? "Empty"}");
            };
            PushText("Added handler for Application.logMessageReceived.");
            Application.quitting += () =>
            {
                PushText($"Quitting Application...");
                PushText($"Disposing log stream in: {fileInfo.FullName}...");
                DisposeStream();
            };
            PushText("Added handler for Application.quitting.");
        }

        private void PushCreation()
        {
            PushText(
                $"Initialized Local App Data from LocalVariables, with {Globals.Variables.Files.Count.ToString()} preexisting files." +
                $"\n    --Folders\n        ---{string.Join(",\n        ---", Globals.Variables.Folders.Values.Select(d => d.Name + " | LWT: " + d.LastWriteTimeUtc))}" +
                $"\n    --Files  \n        ---{string.Join(",\n        ---", Globals.Variables.Files.Values.Select(f => f.Name + " | " + f.Length + " bytes"))}");
            PushText("Finished PyroConsoleView Setup!");
        }

        public void PushText(string message)
        {
            var msg = $"-{message}";
            TextMesh.text += msg;
            Stream.WriteLine(msg);
            Stream.Flush();
        }

        public void DisposeStream()
        {
            Stream.Dispose();
        }
    }
}