using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.UI;

namespace Pyro.Nc.Parsing
{
    [Serializable]
    public class ValueStorage                         
    {
        public DirectoryInfo StorageDirectory { get; set; }
        public Dictionary<int, BaseCommand> GCommands { get; set; }
        public Dictionary<int, BaseCommand> MCommands { get; set; }
        public Dictionary<string, BaseCommand> ArbitraryCommands { get; set; }
        public List<string> Parameters { get; set; }
        public List<BaseCommand> Past { get; set; }
        private ValueStorage()
        {
            CommandHelper.Storage = this;
            Parameters = new List<string>()
            {
                "X",
                "Y",
                "Z",
                "I",
                "J",
                "K",
                "R",
                "CR"
            };
        }

        public BaseCommand FetchGCommand(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var flag0 = code[0] is 'G' or 'g';
            var flag1 = int.TryParse(new string(code.Skip(1).ToArray()), out var num);
            if (flag0 && flag1 && GCommands.TryGetValue(num, out var ic))
            {
                return ic;
            }

            return null;
        }
        
        public BaseCommand FetchMCommand(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var flag0 = code[0] is 'M' or 'm';
            var flag1 = int.TryParse(new string(code.Skip(1).ToArray()), out var num);
            if (flag0 && flag1 && MCommands.TryGetValue(num, out var ic))
            {
                return ic;
            }

            return null;
        }
        
        public BaseCommand FetchArbitraryCommand(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            if (!ArbitraryCommands.TryGetValue(code.ToUpper(), out var ic))
            {
                ref var kvp = ref CommandHelper._cachedKvp;
                if (code.Contains(kvp!.Value.Key))
                {
                    return kvp!.Value.Value;
                }
            }

            // if (ic is null)
            // {
            //     PyroConsoleView.PushTextStatic($"ACommand '{code}' does not exist in the dictionary!");
            //
            //     return null;
            // }
            return ic;
        }

        public UnresolvedCommand FetchUnresolved(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var uc = new UnresolvedCommand(Globals.Tool, new GCommandParameters(0, 0, 0));
            var p = uc.Parameters;

            var s = code.Split(' ');
            bool flag = false;
            foreach (var s1 in s)
            {
                var key = char.ToUpperInvariant(s1[0]).ToString();
                if (p.Values.ContainsKey(key) && float.TryParse(s1.Substring(1), out var val))
                {
                    p.Values[key] = val;
                    flag = true;
                }
            }

            if (flag)
            {
                return uc;
            }
            return null;
        }

        public BaseCommand TryGetCommand(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var upper = code.ToUpper();
            BaseCommand command = FetchGCommand(upper);
            command ??= FetchMCommand(upper);
            command ??= FetchArbitraryCommand(upper);
            command ??= FetchUnresolved(upper);
            if (command is not null)
            {
                return command;
            }
            if (upper.StartsWith("CYCLE"))
            {
                var cycle = new Cycle(Globals.Tool, new ArbitraryCommandParameters());
                
                return cycle;
            }
            

            return null;
        }

        public static ValueStorage CreateFromFile(ToolBase toolBase)
        {                         
            if (CommandHelper.Storage is not null)
            {
                return CommandHelper.Storage;
            }
            ValueStorage strg = new ValueStorage();
            CommandHelper.Storage = strg;
            strg.CreateLocalLowDir();
            string fullPath = $"{strg.StorageDirectory.FullName}\\{CommandIDPath}";
            var separator1 = "##G";
            var separator2 = "##M";
            var separator3 = "##A";
            var spaceSeparator = '|';
            var typePrefixG = "Pyro.Nc.Parsing.GCommands.G";
            var typePrefixM = "Pyro.Nc.Parsing.MCommands.M";
            var typePrefixA = "Pyro.Nc.Parsing.ArbitraryCommands.";
            string[] lines = File.ReadAllLines(fullPath);
            var gcoms = lines.TakeWhile(x => x != separator1).Where(y => !y.Contains("##") && !y.Contains("//")).ToArray();
            var mcoms = lines.SkipWhile(x => x != separator1).TakeWhile(y => y != separator2).Where(z => !z.Contains("##") && !z.Contains("//")).ToArray();
            var acoms = lines.SkipWhile(x => x != separator2).TakeWhile(y => y != separator3).Where(z => !z.Contains("##") && !z.Contains("//")).ToArray();

            strg.GCommands = gcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), v => CreateGCommand(toolBase, typePrefixG, v, spaceSeparator)
                                                );
            strg.MCommands = mcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), 
                                                v => CreateMCommand(toolBase, typePrefixM, v, spaceSeparator));
            strg.ArbitraryCommands = acoms.ToDictionary(k => k.Split(spaceSeparator)[1], 
                                                        v => CreateOtherCommand(toolBase, typePrefixA, v, spaceSeparator));
            
            var types = CustomAssemblyManager.Self.ImportedAssemblies.Select(x => x.GetTypes()).SelectMany(t => t).ToArray();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(BaseCommand))
                {
                    var str = type.Name;
                    try
                    {
                        if (str[0] == 'G' && char.IsDigit(str[1]))
                        {
                            var key = int.Parse(type.Name.Remove(0, 1));
                            if (strg.GCommands.ContainsKey(key))
                            {
                                continue;
                            }
                            strg.GCommands.Add(key, Activator.CreateInstance(type, toolBase, new GCommandParameters(0, 0, 0)) as BaseCommand); 
                        }
                        else if (str[0] == 'M' && char.IsDigit(str[1]))
                        {
                            var key = int.Parse(type.Name.Remove(0, 1));
                            if (strg.MCommands.ContainsKey(key))
                            {
                                continue;
                            }
                            strg.MCommands.Add(key, Activator.CreateInstance(type, toolBase, new MCommandParameters()) as BaseCommand); 
                        }
                        else
                        {
                            if (strg.ArbitraryCommands.ContainsKey(type.Name))
                            {
                                continue;
                            }
                            strg.ArbitraryCommands.Add(type.Name, Activator.CreateInstance(type, toolBase, new ArbitraryCommandParameters()) as BaseCommand);
                        }
                    }
                    catch (Exception e)
                    {
                        Globals.Console.Push("An error has occured whilst adding commands to the database:",
                            $"Value to add: \"{str}\"");
                        Globals.Console.Push($"Descriptive look: {e}");
                    }
                }
            }
            
            CommandHelper._cachedKvp ??= strg.ArbitraryCommands.FirstOrDefault(kvp => kvp.Value.GetType() == typeof(Comment));
            return strg;
        }

        private static BaseCommand CreateGCommand(ToolBase toolBase, string typePrefixG, string v, char spaceSeparator)
        {
            BaseCommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixG + v.Split(spaceSeparator)[0];
                type = Type.GetType(typeFullName);
                instance = type is null ? null
                    : Activator.CreateInstance(type, toolBase, new GCommandParameters(0, 0, 0)) as BaseCommand;
                instance.Family = Group.GCommand;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateGCommand", e.Message,
                                               $"typeFullName: {typeFullName}",
                                               $"Type: IsNull[{(type == null).ToString()}]");
                return new UnresolvedCommand(toolBase, new ArbitraryCommandParameters());
            }

            return instance;
        }

        private static BaseCommand CreateMCommand(ToolBase toolBase, string typePrefixM, string v, char spaceSeparator)
        {
            BaseCommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixM + v.Split(spaceSeparator)[0]; 
                type = Type.GetType(typeFullName);
                instance = type is null ? null : Activator.CreateInstance(type, toolBase, new MCommandParameters()) as BaseCommand;
                instance.Family = Group.MCommand;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateMCommand", e.Message,
                                               $"typeFullName: {typeFullName}", 
                                               $"Type: {type}");
                return new UnresolvedCommand(toolBase, new ArbitraryCommandParameters());
            }

            return instance;
        }

        private static BaseCommand CreateOtherCommand(ToolBase toolBase, string typePrefixA, string v, char spaceSeparator)
        {
            BaseCommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixA + v.Split(spaceSeparator)[0];
                type = Type.GetType(typeFullName);
                instance = type is null ? null
                    : Activator.CreateInstance(type, toolBase, new ArbitraryCommandParameters()) as BaseCommand;
                instance.Family = Group.Other;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateOtherCommand", e.Message,
                                               $"typeFullName: {typeFullName}", 
                                               $"Type: {type}");             
                return new UnresolvedCommand(toolBase, new ArbitraryCommandParameters());
            }

            return instance;
        }

        private void CreateLocalLowDir()
        {
            StorageDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PyroNc");
            var fullPath = $"{StorageDirectory.FullName}\\{CommandIDPath}";
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
            }

            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Exists)
            {
                Directory.CreateDirectory(fi.DirectoryName!);
                File.CreateText(fullPath).Dispose();
            }
        }

        private const string CommandIDPath = "Configuration\\commandId.txt";
    }
}