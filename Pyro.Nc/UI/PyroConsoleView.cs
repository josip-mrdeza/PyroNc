using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI.Debug;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI
{
    public class PyroConsoleView : View
    {
        public TextMeshProUGUI TextMesh;
        public StreamWriter Stream;
        public int Count;
        [StoreAsJson]
        public static bool UseFileStream { get; set; }
        private Process Process;
        private object _lock = new object();
        public override void Initialize()
        {
            UpdateGlobals();
            if (UseFileStream)
            {
                var fileInfo = CreateFileStream();
                PushEventCreation(fileInfo);
            }
            else
            {
                var roaming = LocalRoaming.OpenOrCreate("PyroNc\\Logger");
                var exe = "PyroLogger.exe";
                var file = roaming.Files.FirstOrDefault(x => x.Value.Name == exe);
                var psi = new ProcessStartInfo(file.Value.FullName);
                var fn = "IF.intermediate";
                var arr = Process.GetProcessesByName("PyroLogger");
                foreach (var process in arr)
                {
                    UnityEngine.Debug.Log($"Closing process: {process.Id}");
                    if (process.HasExited)
                    {
                        UnityEngine.Debug.Log("Process exited.");
                    }
                    else
                    {
                        process.Kill();
                    }
                    UnityEngine.Debug.Log("Closed process!");
                }
                roaming.Delete(fn);
                Process = Process.Start(psi);
                Stream = new StreamWriter(roaming.AddFileNoLock(fn, FileAccess.Write));
                Push("Begun STD Stream on console.");
            }   
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
                catch
                {
                    Stream = Globals.Roaming.AddFile("pyroLogClient.txt").CreateText();
                }
            }
            PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewCreatedLogStream, 
                                                     fileInfo.FullName));
            return fileInfo;
        }

        private void UpdateGlobals()
        {
            Globals.Console = this;
            Globals.Roaming ??= LocalRoaming.OpenOrCreate("PyroNc");
            LocalisationManager manager = new LocalisationManager();
            manager.Init();
        }

        private void PushEventCreation(FileInfo fileInfo)
        {
            Application.logMessageReceived += OnApplicationOnLogMessageReceived;
            PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewAddedLogReceivedHandler));
            Application.quitting += ApplicationOnQuit(fileInfo);
            PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewAddedAppQuitHandler));
        }

        private Action ApplicationOnQuit(FileInfo fileInfo)
        {
            return () =>
            {
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewQuitting));
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewLoggingRuntimeValues));
                List<string> typeObjs = new List<string>();
                StringBuilder builder = new StringBuilder();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                                          .Where(x => x.GetName().Name.StartsWith("Pyro")).ToArray();
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewAssemblyLoad, assemblies.Length.ToString()));
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewDisposingLogStream,
                                                         fileInfo.FullName));
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.GenericMessage, 
                                                         $"Average fps: {FpsCounter.averageFps.ToString(CultureInfo.InvariantCulture)}"));
                DisposeStream();
                return;
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
                                typeObjs.Clear();

                                //ignored
                            }
                        }

                        if (typeObjs.Count > 0)
                        {
                            PushTextStatic(typeObjs.Prepend(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewTypeLogger,
                                                                type, assemblyName)).ToArray());
                            typeObjs.Clear();
                        }
                    }
                }

            };
        }

        private void OnApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            Push(condition, stackTrace);
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
                PushTextStatic(Globals.Localisation.Find(Localisation.MapKey.ConsoleViewFinishSetup));
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
            builder.Append(message.FirstOrDefault());
            for (int i = 1; i < message.Length; i++)
            {
                builder.Append('\n');
                builder.Append("    --");
                builder.Append(message[i]);
                //builder.Append('\n');
            }

            //builder.AppendLine("<-------------");
            var str = builder.ToString();
            Globals.Console.PushText(str);
            Globals.InvokeOnLog(str);
        }

        public void DisposeStream()
        {
            Stream.Close();
            Stream.Dispose();
            UnityEngine.Debug.Log("Killing console logger process.");
            Process?.Kill();
            if (!Globals.IsNetworkPresent)
            {
                return;
            }
        }
    }
}