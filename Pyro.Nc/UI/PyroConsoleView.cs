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
        public int Count;
        private object _lock = new object();

        private void Awake()
        {
            //TextMesh ??= GetComponentInChildren<TextMeshProUGUI>();
            UpdateGlobals();
            var fileInfo = CreateFileStream();
            PushEventCreation(fileInfo);
            //Start();
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

        public void PushText(string message, LogType type = LogType.Log)
        {
            var msg = $"[{DateTime.Now.ToLongTimeString()}] - {message}";
            if (type is LogType.Warning or LogType.Error or LogType.Exception)
            {
                if (Count > 13)
                {
                    TextMesh.text = "Console:\n" + msg + '\n';
                    Count = 0;
                }
                else
                {
                    TextMesh.text += msg + '\n';
                    Count++;
                }
            }

            lock (_lock)
            {
                Stream.WriteLine(msg);
                Stream.Flush();
            }
        }

        public static void PushTextStatic(string message, LogType type = LogType.Log)
        {
            if (Globals.Console is null)
            {
                return;
            }
            
            Globals.Console.PushText(message, type);
        }

        public void DisposeStream()
        {
            Stream.Dispose();
        }
    }
}