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
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Builder;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Algos;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Configuration.Managers;

public class CustomAssemblyManager : InitializerRoot
{
    [StoreAsJson]
    public static bool EnablePluginUpdates { get; set; } = true;

    public static Dictionary<int, IMillAlgorithm> Algorithms;
    public static CustomAssemblyManager Self;
    public List<Assembly> ImportedAssemblies;
    public List<IPyroPlugin> Plugins;
    public bool IsAsync { get; } = true;
    public bool DisableAutoInit { get; } = true;

    public override async Task InitializeAsync()
    {
        Self = this;
        ImportedAssemblies = new List<Assembly>();
        LocalRoaming compilerFolder = LocalRoaming.OpenOrCreate("PyroNc\\Compiler");
        AddMissingCompilerFiles(compilerFolder);
        await Task.Run(CompilePlugins);
        InitializeAllPlugins();
        ValueStorage.AddCommandsFromPlugins();
        Algorithms = FindAlgorithms();
    }

    private Dictionary<int, IMillAlgorithm> FindAlgorithms()
    {
        var algos = new Dictionary<int, IMillAlgorithm>();
        List<Type> types = new List<Type>();
        types.AddRange(ImportedAssemblies.SelectMany(x => x.GetTypes()));
        types.AddRange(new Type[]
        {
            typeof(SimpleCutAlgorithm),
            typeof(VertexBoxHashAlgorithm),
            typeof(CompiledLineHashAlgorithm),
            typeof(AdditiveCompiledLineHashCutAlgorithm),
            typeof(GPUAcceleratedLineHashAlgorithm)
        });
        foreach (var type in types)
        {
            if (type.GetInterface(nameof(IMillAlgorithm)) != null)
            {
                try
                {
                    var obj = (IMillAlgorithm)Activator.CreateInstance(type);
                    var id = type.GetProperty(nameof(IMillAlgorithm.Id));
                    var idValue = (int) id.GetValue(obj);
                    algos.Add(idValue, obj);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        return algos;
    }

    private void CompilePlugins()
    {
        var roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Plugins");
        Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerTitleBrackets));
        foreach (var dir in roaming.ListAllDirectories().ToArray())
        {
            try
            {
                var files = dir.GetFiles();
                if (files.FirstOrDefault(x => x.Name == "plugin.disable") != null)
                {
                    continue;
                }

                var assemblyName = dir.Name;
                if (files.Length == 0)
                {
                    Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerDirectoryEmpty,
                                                                   dir.Name));

                    continue;
                }

                var assemblyFile = files.FirstOrDefault(f => f.Name == "{0}.dll".Format(assemblyName));
                if (assemblyFile != null)
                {
                    Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerAlreadyCompiled,
                                                                   dir.Name));
                    ImportedAssemblies.Add(Assembly.LoadFrom(assemblyFile.FullName));

                    continue;
                }
                var compilerRoaming = LocalRoaming.OpenOrCreate("PyroNc\\Compiler");
                var exe = compilerRoaming.Files.Single(x => x.Key == "PyroCompiler.exe").Value;
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = exe.FullName;
                startInfo.Arguments =
                    $"\"{assemblyName}\" \"{dir.FullName}\" {string.Join(" ", files.Select(x => $"\"{x.FullName}\"").ToArray())}";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                var process = Process.Start(startInfo);
                Task.Run(() =>
                {             
                    while (!process.HasExited)
                    {
                        Globals.Console.Push($"[PyroCompiler]: {process.StandardOutput.ReadLine()}");
                    }
                });
                process.Exited += (sender, args) =>
                {
                    Globals.Console.Push(process.StandardOutput.ReadToEnd());
                };
                process.WaitForExit();
                var assembly = Assembly.LoadFrom(Path.Combine(dir.FullName, assemblyName) + ".dll");
                ImportedAssemblies.Add(assembly);
                Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerLoadedAssembly,
                                                               assemblyName));
            }
            catch (Exception e)
            {
                Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerFailed, e.Message));
            }
        }

        Globals.Console.Push(new string[]
                             {
                                 Globals.Localisation.Find(
                                     Localisation.MapKey.CustomAssemblyManagerCompletedAndImportedCount,
                                     ImportedAssemblies.Count.ToString())
                             }
                             .Concat(ImportedAssemblies.Select(
                                         a => a.GetName().Name)).ToArray());
    }

    private static void AddMissingCompilerFiles(LocalRoaming compilerFolder)
    {
        string path = null;
        string fullPath = null;
        DirectoryInfo di = null;
        FileInfo[] compilerFiles = null;
        try
        {
            path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
            fullPath = Path.Combine(path, "bin\\Debug");
            di = new DirectoryInfo(fullPath);
            compilerFiles = di.GetFiles();
        }
        catch (Exception e)
        {
            Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerFailed,
                                                           $"Path={path}", e.Message));
            Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.GenericHandledError,
                                                           "Cannot find compiler path!"));

            return;
        }

        foreach (var file in compilerFiles)
        {
            if (!compilerFolder.Exists(file.Name))
            {
                compilerFolder.AddFile(file.Name, File.ReadAllBytes(file.FullName));

                // Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerAddMissingFile, 
                //                                                file.Name, 
                //                                                compilerFolder.Site));
            }
        }
    }
    private void InitializeAllPlugins()
    {
        if (ImportedAssemblies == null)
        {
            return;
        }

        Plugins = new List<IPyroPlugin>();
        foreach (var assembly in ImportedAssemblies)
        {
            var types = assembly.GetTypes().Where(x =>
            {
                return x.GetInterface(nameof(IPyroPlugin)) != null;
            }).ToArray();
            foreach (var type in types)
            {
                try
                {
                    var obj = Activator.CreateInstance(type);
                    IPyroPlugin plugin = obj as IPyroPlugin;
                    Plugins.Add(plugin);
                    plugin.InitializePlugin();
                }
                catch (Exception e)
                {
                    Globals.Console.Push($"~[{assembly.GetName().Name}.dll]: Could not initialize type '{type.Name}'\n" +
                                         $"----Reason: {e.Message}.~");
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!EnablePluginUpdates || Plugins == null)
        {
            return;
        }
        foreach (var pyroPlugin in Plugins)
        {
            try
            {
                pyroPlugin.Update();
            }
            catch (Exception e)
            {
                Globals.Console.Push($"~[{pyroPlugin.GetType().Name}, Update()]: {e.Message}!~");
            }
        }
    }

    private void OnWrite(object o, DataReceivedEventArgs args)
    {
        Globals.Console.Push(Globals.Localisation.Find(Localisation.MapKey.CustomAssemblyManagerCompilerComMessage,
                                 args.Data));
    }
}