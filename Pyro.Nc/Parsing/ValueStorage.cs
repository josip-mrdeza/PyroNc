using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using TrCore;
using TrCore.Logging;
using TrCore.Reflection;

namespace Pyro.Nc.Parsing
{
    public class ValueStorage                         
    {
        public DirectoryInfo StorageDirectory
        {
            get;
            set;
        }

        public Dictionary<int, ICommand> GCommands { get; set; }
        public Dictionary<int, ICommand> MCommands { get; set; }
        public Dictionary<string, ICommand> ArbitraryCommands { get; set; }
        
        public Queue<ICommand> Recents { get; set; }
        
        private ValueStorage()
        {
            CommandHelper._storage = this;
        }

        public ICommand FetchGCommand(string code)
        {
            var flag0 = code[0] is 'G' or 'g';
            var flag1 = int.TryParse(new string(code.Skip(1).ToArray()), out var num);
            if (flag0 && flag1 && GCommands.TryGetValue(num, out var ic))
            {
                return ic.GuardNull($"GCommand '{code}' does not exist in the dictionary!").Copy();
            }

            return null;
        }
        
        public ICommand FetchMCommand(string code)
        {
            var flag0 = code[0] is 'M' or 'm';
            var flag1 = int.TryParse(new string(code.Skip(1).ToArray()), out var num);
            if (flag0 && flag1 && MCommands.TryGetValue(num, out var ic))
            {
                return ic.GuardNull($"MCommand '{code}' does not exist in the dictionary!").Copy();
            }

            return null;
        }
        
        public ICommand FetchArbitraryCommand(string code)
        {
            if (!ArbitraryCommands.TryGetValue(code.ToUpper(), out var ic))
            {
                ref var kvp = ref CommandHelper._cachedKvp;
                if (code.Contains(kvp!.Value.Key))
                {
                    return kvp!.Value.Value.Copy();
                }
                return null;
            }
            
            return ic.GuardNull($"ACommand '{code}' does not exist in the dictionary!").Copy();
        }

        public ICommand TryGetCommand(string code)
        {
            var upper = code.ToUpper();
            ICommand command = FetchGCommand(upper);
            command ??= FetchMCommand(upper);
            command ??= FetchArbitraryCommand(upper);

            return command;
        }

        public static ValueStorage CreateFromFile(ITool tool)
        {                         
            if (CommandHelper._storage is not null)
            {
                return CommandHelper._storage;
            }
            ValueStorage strg = new ValueStorage();
            CommandHelper._storage = strg;
            strg.Recents = new Queue<ICommand>();
            strg.CreateLocalLowDir();
            ManagerStorage.InitAll();

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

            strg.GCommands = gcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), v =>
            {
                var typeFullName = typePrefixG + v.Split(spaceSeparator)[0];
                var type = Type.GetType(typeFullName);
                var instance = type is null ? null : Activator.CreateInstance(type, tool, new GCommandParameters(0, 0, 0)) as ICommand;
                return instance;
            });
            
            strg.MCommands = mcoms.ToDictionary(k => int.Parse(k.Split(spaceSeparator)[0]), v =>
            {
                var typeFullName = typePrefixM + v.Split(spaceSeparator)[0];
                var type = Type.GetType(typeFullName);
                var instance = type is null ? null : Activator.CreateInstance(type, tool, new MCommandParameters()) as ICommand;
                return instance;
            });
            
            strg.ArbitraryCommands = acoms.ToDictionary(k => k.Split(spaceSeparator)[1], v =>
            {
                var typeFullName = typePrefixA + v.Split(spaceSeparator)[0];
                var type = Type.GetType(typeFullName);
                var instance = type is null ? null : Activator.CreateInstance(type, tool, new ArbitraryCommandParameters()) as ICommand;
                return instance;
            }); 
            
            CommandHelper._cachedKvp ??= strg.ArbitraryCommands.FirstOrDefault(kvp => kvp.Value.GetType() == typeof(Comment));
            return strg;
        }

        private void CreateLocalLowDir()
        {
            // void CreateMissingFile(string s)
            // {
            //     File.ReadAllBytes($"{}\\{CommandIDPath}").Do(x =>
            //     {
            //         File.Create(s).Do(y =>
            //         {
            //             y.Write(x, 0, x.Length);
            //             y.Dispose();
            //         });
            //     });
            // }

            StorageDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PyroNc");
            var fullPath = $"{StorageDirectory.FullName}\\{CommandIDPath}";
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
                //CreateMissingFile(fullPath);
            }

            if (!File.Exists(fullPath))
            {
                //CreateMissingFile(fullPath);
                File.CreateText(fullPath).Dispose();
            }
        }

        private const string CommandIDPath = "Configuration\\commandId.txt";
    }
}