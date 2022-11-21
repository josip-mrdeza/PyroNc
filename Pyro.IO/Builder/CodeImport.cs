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
    private static string[] _assemblyPaths = new string[]
    {
        typeof(Task).Assembly.Location,
        typeof(Enumerable).Assembly.Location,
        "UnityEngine.dll",
        "UnityEngine.Core.dll",
        typeof(CodeImport).Assembly.Location,
        typeof(SharedMemory).Assembly.Location,
        typeof(Operations).Assembly.Location,
        "Pyro.Nc.dll",
        "Pyro.Net.dll",
        typeof(PyroDispatcher).Assembly.Location
    };

    private static string[] _assemblyNames = _assemblyPaths.Select(x =>
    {
        var i = x.LastIndexOf('\\');
        return x.Remove(0, i == -1 ? 0 : i + 1);
    }).ToArray();

    public static void AddAllImports(params string[] filePaths)
    {
        foreach (var filePath in filePaths)
        {
            var lines = File.ReadLines(filePath).Where(x=> !string.IsNullOrWhiteSpace(x)).ToArray();
            IEnumerable<string> l2 = lines.Where(x => !x.StartsWith("using"));
            l2 = l2.Prepend("using System;\nusing System.Threading.Tasks;\nusing System.Reflection;\nusing System.IO;\nusing System.Linq;" +
                            "\nusing System.Collections;\nusing System.Collections.Generic;\n\n");
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
        codeProvider.Dispose();
        var cr = codeProvider.CompileAssemblyFromSource(new CompilerParameters(_assemblyPaths.Where(x => File.Exists(x)).ToArray(), 
                                                                               Path.Combine(path, assemblyName + ".dll")), 
                                                        filePaths.Where(f => !f.EndsWith(".dll")).Select(x => File.ReadAllText(x)).ToArray());
        codeProvider.Dispose();
        return cr;
    }
}