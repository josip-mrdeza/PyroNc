using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Pyro.IO;
using Pyro.IO.Builder;

namespace PyroCompiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CompilerResults results = null;
            Console.WriteLine("Compiling files...");
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc\\Compiler\\References");
            try
            {
                var assemblyName = args[0];
                var pathToDirectory = args[1];
                results = CodeImport.ImportTextLibrary(assemblyName, pathToDirectory,
                                                       roaming.Files.Select(f => f.Value.FullName).ToArray(),
                                                       args.Skip(2).ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
            }
            Console.WriteLine("Finished!");
        }
    }
}