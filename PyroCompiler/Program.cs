using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Pyro.IO;
using Pyro.IO.Builder;

namespace PyroCompiler
{
    internal class Program
    {
        //public static StreamWriter st;
        public static void Main(string[] args)
        {
            CompilerResults results = null;
            //var process = Process.GetProcessesByName("PyroLogger").FirstOrDefault();
            Console.WriteLine("Compiling files...");
            try
            {
                var assemblyName = args[0];
                var pathToDirectory = args[1];
                var lr = LocalRoaming.OpenOrCreate($"PyroNc\\Configuration\\Plugins\\{assemblyName}\\Dependencies");
                results = CodeImport.ImportTextLibrary(assemblyName, pathToDirectory,
                                                       lr.Files.Select(f => f.Value.FullName).ToArray(),
                                                       args.Skip(2).ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"~{e}~");
                //st?.WriteLine(e);
            }
            finally
            {
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine($"~{error.ErrorText}~");
                        //st?.WriteLine(error.ErrorText);
                    }
                }
            }
            //st?.WriteLine("Finished!");
            //st?.Dispose();
            Console.Write("Finished!");
        }
    }
}