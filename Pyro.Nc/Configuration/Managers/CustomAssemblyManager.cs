using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

                    var assembly = files.FirstOrDefault(f => f.Name == "{0}.dll".Format(assemblyName));
                    if (assembly != null)
                    {
                        Globals.Console.Push("[CustomAssemblyManager] - Directory '{0}' has already been compiled into an assembly, skipping!".Format(dir.Name));
                        ImportedAssemblies.Add(Assembly.LoadFrom(assembly.FullName));
                        continue;
                    }
                    Globals.Console.Push(new string[]{"[CustomAssemblyManager] - Attempting to build assembly with references:"}.Concat(CodeImport.AssemblyPaths).ToArray());
                    var results = CodeImport.ImportTextLibrary(assemblyName, dir.FullName, files.Select(x => x.FullName).ToArray());
                    if (!results.Errors.HasErrors)
                    {
                        ImportedAssemblies.Add(results.CompiledAssembly);
                        Globals.Console.Push("[CustomAssemblyManager] - Finished compiling assembly '{0}', with no errors!".Format(assemblyName));
                    }
                    else
                    {
                        Globals.Console.Push("[CustomAssemblyManager] - Failed to compile assembly '{0}', {1} errors occured!".Format(assemblyName, results.Errors.Count));
                        Globals.Console.Push(results.Errors.Cast<object>().Select(c => ((CompilerError)c).ErrorText).ToArray());
                    }
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
}