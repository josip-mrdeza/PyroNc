using System;
using System.IO;
using System.Linq;
using System.Text;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Statistics;
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
            var fileInfo = Globals.Roaming.AddFile("pyroLog.txt");
            Stream = fileInfo.CreateText();
            PushTextStatic($"Created Log Stream in: {fileInfo.FullName}.");
            return fileInfo;
        }

        private void UpdateGlobals()
        {
            Globals.Console = this;
            Globals.Roaming ??= LocalRoaming.OpenOrCreate("PyroNc");
        }

        private void PushEventCreation(FileInfo fileInfo)
        {
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                PushTextStatic($"LogType.{type}:\n    --{condition ?? "Empty"}\n    --StackTrace:{stackTrace ?? "Empty"}");
            };
            PushTextStatic("Added handler for Application.logMessageReceived.");
            Application.quitting += () =>
            {
                PushTextStatic($"Quitting Application...");
                PushTextStatic($"Disposing log stream in: {fileInfo.FullName}...");
                DisposeStream();
            };
            PushTextStatic("Added handler for Application.quitting.");
        }

        private void PushCreation()
        {
            //string.Join(",\n        ---", Globals.Variables.Folders.Values.Select(d => d.Name + " | LWT: " + d.LastWriteTimeUtc))
            //string.Join(",\n        ---", Globals.Variables.Files.Values.Select(f => f.Name + " | " + f.Length + " bytes"))
            PushTextStatic(
                $"Initialized Local App Data from LocalVariables, with {Globals.Roaming.Files.Count.ToString()} preexisting files.",
                string.Join(",\n    --", Globals.Roaming.Folders.Values.Select(d => "[Folder]" + d.Name + " | LWT: " + d.LastWriteTimeUtc)),
                "--------",
                string.Join(",\n    --",
                            Globals.Roaming.Files.Values.Select(f => "[File]" + f.Name + " | " + f.Length + " bytes")));
                PushTextStatic("Finished PyroConsoleView Setup!");
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

        public static void PushTextStatic(params string[] message)
        {
            if (Globals.Console is null)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(message.FirstOrDefault());
            for (int i = 1; i < message.Length; i++)
            {
                builder.Append("    --");
                builder.Append(message[i]);
                builder.Append('\n');
            }

            builder.AppendLine("<-------------");
            Globals.Console.PushText(builder.ToString());
        }

        public void DisposeStream()
        {
            Stream.Dispose();
            if (!Globals.IsNetworkPresent)
            {
                return;
            }

            //Collector.SendLogStatisticAsync();
        }
    }
}