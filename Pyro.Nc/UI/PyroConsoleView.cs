using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class PyroConsoleView : View
    {
        public TextMeshProUGUI TextMesh;
        public StreamWriter Stream;
        public int Count;
        private object _lock = new object();
        public override void Initialize()
        {
            UpdateGlobals();
            var fileInfo = CreateFileStream();
            PushEventCreation(fileInfo);
            PushCreation();
            base.Initialize();
        }

        private FileInfo CreateFileStream()
        {
            var fileInfo = Globals.Roaming.AddFile("pyroLog.txt");
            lock (_lock)
            {
                try
                {
                    Stream = fileInfo.CreateText();
                }
                catch (Exception e)
                {
                    Stream = Globals.Roaming.AddFile("pyroLogClient.txt").CreateText();
                }
            }
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
            Application.logMessageReceived += OnApplicationOnLogMessageReceived;
            PushTextStatic("Added handler for Application.logMessageReceived.");
            Application.quitting += ApplicationOnQuit(fileInfo);
            PushTextStatic("Added handler for Application.quitting.");
        }

        private Action ApplicationOnQuit(FileInfo fileInfo)
        {
            return () =>
            {
                PushTextStatic($"Quitting Application...");
                PushTextStatic("Logging all available runtime values...");
                List<string> typeObjs = new List<string>();
                StringBuilder builder = new StringBuilder();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                                          .Where(x => x.GetName().Name.StartsWith("Pyro")).ToArray();
                PushTextStatic($"Found {assemblies.Length} assemblies...");
                foreach (var assembly in assemblies)
                {
                    var typesInAssembly = assembly.GetTypes();
                    var assemblyName = assembly.GetName().Name;
                    foreach (var type in typesInAssembly)
                    {
                        var staticMembersInType = type.GetMembers(BindingFlags.Public | BindingFlags.Static);
                        foreach (var memberInfo in staticMembersInType)
                        {
                            try
                            {
                                var fieldInfo = memberInfo as FieldInfo;
                                object value;
                                if (fieldInfo != null)
                                {
                                    value = fieldInfo.GetValue(null);
                                }
                                else
                                {
                                    var prop = memberInfo as PropertyInfo;
                                    if (prop is null)
                                    {
                                        continue;
                                    }

                                    value = prop.GetValue(null);
                                }

                                if (value is ICollection arr)
                                {
                                    if (arr.Count <= 100)
                                    {
                                        builder.Append('\'').Append(memberInfo.Name).Append('\'').Append(':');
                                        var enumerator = arr.GetEnumerator();
                                        while (enumerator.MoveNext())
                                        {
                                            builder.Append("\n    --'").Append(enumerator.Current).Append('\'');
                                        }

                                        typeObjs.Add(builder.ToString());
                                        builder.Clear();
                                    }
                                }
                                else
                                {
                                    typeObjs.Add($"\"{memberInfo.Name}\":\"" + value + '\"');
                                }
                            }
                            catch
                            {
                                //ignored
                            }
                        }

                        if (typeObjs.Count > 0)
                        {
                            PushTextStatic(typeObjs.Prepend($"Type '{type}' in assembly '{assemblyName}':").ToArray());
                            typeObjs.Clear();
                        }
                    }
                }
                PushTextStatic($"Disposing log stream in: {fileInfo.FullName}...");
                DisposeStream();
            };
        }

        private void OnApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            Push(condition, stackTrace.Substring(0, 100).Insert(100, "..."));
            //await Collector.HttpClient.PostAsync(Collector.BaseAddress + $"/error/{Collector.Info.Name}_ERR", new StringContent(condition));
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