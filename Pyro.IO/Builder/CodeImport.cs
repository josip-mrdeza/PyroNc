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

namespace Pyro.IO.Builder;

public static class CodeImport
{
    private static Assembly[] _LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

    private static Dictionary<string, MethodInfo> _availableSwitchableCustomMethodImpls;

    public static CompilerResults ImportTextLibrary(string assemblyName, string path, string[] referencePaths, params string[] fileSourcePaths)
    {
        CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        //AddAllImports(fileSourcePaths);
        var cr = codeProvider.CompileAssemblyFromSource(new CompilerParameters(referencePaths, assemblyName), 
                                                        fileSourcePaths.Where(f => !f.EndsWith(".dll")).Select(x => File.ReadAllText(x)).ToArray());
        File.Copy(cr.PathToAssembly, Path.Combine(path, assemblyName + ".dll"));
        codeProvider.Dispose();
        return cr;
    }
    public static void SwitchMethodImplsForCustom(Assembly customAssembly)
    {
        throw new NotImplementedException();
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