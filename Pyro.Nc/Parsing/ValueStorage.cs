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
using Pyro.Nc.UI;

namespace Pyro.Nc.Parsing
{
    [Serializable]
    public class ValueStorage                         
    {
        public DirectoryInfo StorageDirectory { get; set; }
        public Dictionary<int, ICommand> GCommands { get; set; }
        public Dictionary<int, ICommand> MCommands { get; set; }
        public Dictionary<string, ICommand> ArbitraryCommands { get; set; }
        public List<string> Parameters { get; set; }
        public List<ICommand> Past { get; set; }
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

        public ICommand FetchGCommand(string code)
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
        
        public ICommand FetchMCommand(string code)
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
        
        public ICommand FetchArbitraryCommand(string code)
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

        public ICommand TryGetCommand(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var upper = code.ToUpper();
            ICommand command = FetchGCommand(upper);
            command ??= FetchMCommand(upper);
            command ??= FetchArbitraryCommand(upper);
            command ??= FetchUnresolved(upper);
            return command;
        }

        public static ValueStorage CreateFromFile(ITool tool)
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

            strg.GCommands = gcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), v => CreateGCommand(tool, typePrefixG, v, spaceSeparator)
                                                );
            strg.MCommands = mcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), 
                                                v => CreateMCommand(tool, typePrefixM, v, spaceSeparator));
            strg.ArbitraryCommands = acoms.ToDictionary(k => k.Split(spaceSeparator)[1], 
                                                        v => CreateOtherCommand(tool, typePrefixA, v, spaceSeparator));
            CommandHelper._cachedKvp ??= strg.ArbitraryCommands.FirstOrDefault(kvp => kvp.Value.GetType() == typeof(Comment));
            return strg;
        }

        private static ICommand CreateGCommand(ITool tool, string typePrefixG, string v, char spaceSeparator)
        {
            ICommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixG + v.Split(spaceSeparator)[0];
                type = Type.GetType(typeFullName);
                instance = type is null ? null
                    : Activator.CreateInstance(type, tool, new GCommandParameters(0, 0, 0)) as ICommand;
                instance.Family = Group.GCommand;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateGCommand", e.Message,
                                               $"typeFullName: {typeFullName}",
                                               $"Type: IsNull[{(type == null).ToString()}]");
                return new UnresolvedCommand(tool, new ArbitraryCommandParameters());
            }

            return instance;
        }

        private static ICommand CreateMCommand(ITool tool, string typePrefixM, string v, char spaceSeparator)
        {
            ICommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixM + v.Split(spaceSeparator)[0]; 
                type = Type.GetType(typeFullName);
                instance = type is null ? null : Activator.CreateInstance(type, tool, new MCommandParameters()) as ICommand;
                instance.Family = Group.MCommand;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateMCommand", e.Message,
                                               $"typeFullName: {typeFullName}", 
                                               $"Type: {type}");
                return new UnresolvedCommand(tool, new ArbitraryCommandParameters());
            }

            return instance;
        }

        private static ICommand CreateOtherCommand(ITool tool, string typePrefixA, string v, char spaceSeparator)
        {
            ICommand instance;
            string typeFullName = null;
            Type type = null;
            try
            {
                typeFullName = typePrefixA + v.Split(spaceSeparator)[0];
                type = Type.GetType(typeFullName);
                instance = type is null ? null
                    : Activator.CreateInstance(type, tool, new ArbitraryCommandParameters()) as ICommand;
                instance.Family = Group.Other;
            }
            catch (Exception e)
            {
                PyroConsoleView.PushTextStatic("An exception has occured in ValueStorage::CreateOtherCommand", e.Message,
                                               $"typeFullName: {typeFullName}", 
                                               $"Type: {type}");             
                return new UnresolvedCommand(tool, new ArbitraryCommandParameters());
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