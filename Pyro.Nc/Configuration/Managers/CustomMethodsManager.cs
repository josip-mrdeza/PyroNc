using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Builder;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Managers;

public class CustomMethodsManager : IManager
{
    public List<Assembly> ImportedAssemblies;
    public bool IsAsync { get; } = true;

    public async Task InitAsync()
    {
        ImportedAssemblies = new List<Assembly>();
        var roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration\\Plugins");
        await Task.Run(() =>
        {
            foreach (var dir in roaming.ListAllDirectories().ToArray())
            {
                var files = dir.GetFiles();
                if (files.Length == 0)
                {
                    continue;
                }

                var assemblyName = dir.Name;
                var results = CodeImport.ImportTextLibrary(assemblyName, dir.FullName, files.Select(x => x.FullName).ToArray());
                if (!results.Errors.HasErrors)
                {
                    ImportedAssemblies.Add(results.CompiledAssembly);
                }
                else
                {
                    Globals.Console.Push(results.Errors.Cast<object>().Select(c => ((CompilerError)c).ErrorText).ToArray());
                }
            }    
        });
    }

    public void Init() => throw new System.NotImplementedException();
}