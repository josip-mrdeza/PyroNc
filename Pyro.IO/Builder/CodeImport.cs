using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Pyro.IO.Memory;
using Pyro.Math;
using Pyro.Threading;

namespace Pyro.IO.Builder;

public static class CodeImport
{
    private static Assembly[] _LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
    private static readonly string[] _assemblyPaths = new string[]
    {
        //typeof(Task).Assembly.Location,
        typeof(Enumerable).Assembly.Location,
        _LoadedAssemblies.First(a => a.GetName().Name == "UnityEngine.CoreModule").Location,
        _LoadedAssemblies.First(a => a.GetName().Name == "UnityEngine").Location,
        typeof(CodeImport).Assembly.Location,
        typeof(SharedMemory).Assembly.Location,
        typeof(Operations).Assembly.Location,
        _LoadedAssemblies.First(a => a.GetName().Name == "Pyro.Nc").Location,
        typeof(PyroDispatcher).Assembly.Location
    };

    private static readonly string[] _assemblyNames = _assemblyPaths.Select(x =>
    {
        var i = x.LastIndexOf('\\');
        return x.Remove(0, i == -1 ? 0 : i + 1);
    }).ToArray();

    private static Dictionary<string, MethodInfo> _availableSwitchableCustomMethodImpls;

    public static string[] AssemblyPaths => _assemblyPaths;
    public static void AddAllImports(params string[] filePaths)
    {
        foreach (var filePath in filePaths)
        {
            var lines = File.ReadLines(filePath).Where(x=> !string.IsNullOrWhiteSpace(x)).ToArray();
            IEnumerable<string> l2 = lines.Where(x => !x.StartsWith("using"));
            l2 = l2.Prepend("using System;\nusing System.Threading.Tasks;\nusing System.Reflection;\nusing System.IO;\nusing System.Linq;" +
                            "\nusing System.Collections;\nusing System.Collections.Generic;\nusing Pyro.Nc.Simulation;\nusing Pyro.Nc.UI;" +
                            "\nusing Pyro.Nc.Pathing;\nusing Pyro.Nc.Parsing;\nusing Pyro.Nc.Parsing.ArbitraryCommands;" +
                            "\nusing Pyro.Nc.Parsing.GCommands;\nusing Pyro.Nc.Parsing.MCommands;\nusing Pyro.Nc.Exceptions;" +
                            "\nusing Pyro.Nc.Configuration;\nusing Pyro.Nc.Configuration.Managers;" +
                            "\nusing Pyro.Nc.Configuration.Sim3D_Legacy;\nusing Pyro.Nc.Configuration.Startup;" +
                            "\nusing UnityEngine;\n//using UnityEngine.CoreModule;\n\n");
            if (lines.Length == 0)
            {
                foreach (var assemblyName in _assemblyNames)
                {
                    if (!File.Exists(assemblyName))
                    {
                        continue;
                    }
                    var str = "using " + assemblyName.Replace(".dll", string.Empty) + ";";
                    l2 = l2.Prepend(str);
                }
            }

            foreach (var assemblyName in _assemblyNames)
            {
                if (!File.Exists(assemblyName))
                {
                    continue;
                }

                var str = "using " + assemblyName.Replace(".dll", string.Empty) + ";";
                l2 = l2.Prepend(str);
            }

            File.WriteAllLines(filePath, l2);
        }
    }
    public static CompilerResults ImportTextLibrary(string assemblyName, string path, params string[] filePaths)
    {
        CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        AddAllImports(filePaths);
        var cr = codeProvider.CompileAssemblyFromSource(new CompilerParameters(_assemblyPaths.Where(x => File.Exists(x)).ToArray(), 
                                                                               Path.Combine(path, assemblyName + ".dll")), 
                                                        filePaths.Where(f => !f.EndsWith(".dll")).Select(x => File.ReadAllText(x)).ToArray());
        codeProvider.Dispose();
        return cr;
    }
    public static void SwitchMethodImplsForCustom(Assembly customAssembly)
    {
        throw new NotImplementedException();
        if (_availableSwitchableCustomMethodImpls is null)
        {
            InitCustomImplMethodsInMainAssembly();
        }

        Type[] t = customAssembly.GetTypes();
        foreach (var type in t)
        {
            foreach (var method in type.GetMethods())
            {
                if (_availableSwitchableCustomMethodImpls.ContainsKey(method.Name))
                {
                    var mi = _availableSwitchableCustomMethodImpls[method.Name];
                    var prop = typeof(MethodInfo).GetProperty("");
                }
            }
        }
    }
    private static void InitCustomImplMethodsInMainAssembly()
    {
        Type[] t = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes()).SelectMany(x => x).ToArray();
        _availableSwitchableCustomMethodImpls = new Dictionary<string, MethodInfo>();
        foreach (var type in t)
        {
            var methods = type.GetMethods(BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<CustomMethodAttribute>() != null)
                {
                    _availableSwitchableCustomMethodImpls.Add(method.Name, method);
                }
            }
        }
    }
}