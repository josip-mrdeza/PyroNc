using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Builder;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Configuration.Managers;

public class CustomAssemblyManager : IManager
{
    public static CustomAssemblyManager Self;
    public List<Assembly> ImportedAssemblies;
    public bool IsAsync { get; } = true;
    public bool DisableAutoInit { get; } = true;

    public async Task InitAsync()
    {
        Self = this;
        ImportedAssemblies = new List<Assembly>();
        LocalRoaming compilerFolder = LocalRoaming.OpenOrCreate("PyroNc\\Compiler");
        var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
        var fullPath = Path.Combine(path, "bin\\Debug");
        DirectoryInfo di = new DirectoryInfo(fullPath);
        var compilerFiles = di.GetFiles();
        foreach (var file in compilerFiles)
        {
            if (!compilerFolder.Exists(file.Name))
            {
                compilerFolder.AddFile(file.Name, File.ReadAllBytes(file.FullName));
                Globals.Console.Push($"Adding missing file: '{file.Name}' to compiler dir '{compilerFolder.Site}'.");
            }
        }
        LocalRoaming roaming2 = LocalRoaming.OpenOrCreate("PyroNc\\Compiler\\References");
        foreach (var assemblyPath in CodeImport.AssemblyPaths)
        {
            roaming2.AddFile(Path.GetFileName(assemblyPath), File.ReadAllBytes(assemblyPath));
        }
        await Task.Run(() =>
        {
            var roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Plugins");
            Globals.Console.Push("[CustomAssemblyManager]");
            foreach (var dir in roaming.ListAllDirectories().ToArray())
            {
                try
                {
                    var files = dir.GetFiles();
                    var assemblyName = dir.Name;
                    if (files.Length == 0)
                    {
                        Globals.Console.Push("[CustomAssemblyManager] - Directory '{0}' is empty, skipping!".Format(dir.Name));
                        continue;
                    }

                    var assemblyFile = files.FirstOrDefault(f => f.Name == "{0}.dll".Format(assemblyName));
                    if (assemblyFile != null)
                    {
                        Globals.Console.Push("[CustomAssemblyManager] - Directory '{0}' has already been compiled into an assembly, skipping!".Format(dir.Name));
                        ImportedAssemblies.Add(Assembly.LoadFrom(assemblyFile.FullName));
                        continue;
                    }
                    Globals.Console.Push(new string[]{"[CustomAssemblyManager] - Attempting to build assembly with references:"}.Concat(CodeImport.AssemblyPaths).ToArray());
                    var compilerRoaming = LocalRoaming.OpenOrCreate("PyroNc\\Compiler");
                    var exe = compilerRoaming.Files.Single(x => x.Key == "PyroCompiler.exe").Value;
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = exe.FullName;
                    startInfo.Arguments = $"\"{assemblyName}\" \"{dir.FullName}\" {string.Join(" ", files.Select(x => $"\"{x.FullName}\"").ToArray())}";
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    var process = Process.Start(startInfo);    
                    process.OutputDataReceived += OnWrite;
                    process.Exited += (sender, args) =>
                    {
                        Globals.Console.Push(process.StandardOutput.ReadToEnd());
                    };
                    process.WaitForExit();
                    var assembly = Assembly.LoadFrom(Path.Combine(dir.FullName, assemblyName) + ".dll");
                    ImportedAssemblies.Add(assembly);
                    Globals.Console.Push($"[CustomAssemblyManager] - Loaded assembly '{assemblyName}'!");
                }
                catch (Exception e)
                {
                    Globals.Console.Push(e.Message);
                }
            }    
            Globals.Console.Push(new string[]{$"[CustomAssemblyManager] - Completed && Imported {ImportedAssemblies.Count} assemblies!"}.Concat(ImportedAssemblies.Select(
                                     a => a.GetName().Name)).ToArray());
        });
    }

    public void Init() => throw new System.NotImplementedException();

    public void OnWrite(object o, DataReceivedEventArgs args)
    {
        Globals.Console.Push($"[PyroCompiler] - Compiler message: \"{args.Data}\".");
    }
}