using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using ICommand = Pyro.Nc.Parsing.GCommands.ICommand;

namespace Pyro.Nc.Parsing
{
    public static class CommandHelper
    {
        internal static ValueStorage _storage;
        public static bool IsTrue<T>(this T obj, Predicate<T> predicate)
        {
            return predicate(obj);
        }
        public static bool IsMatch(this ICommand command1, ICommand command2)
        {
            var flag0 = command1.Parameters == command2.Parameters;
            var flag1 = command1.Description == command2.Description;

            return flag0 && flag1;
        }
        public static bool IsMatch(this ICommand command, string code)
        {
            return code.IdentifyVariables().GatherCommands().IsTrue(t =>
            {
                var flag0 = t.Count == 1;
                if (!flag0)
                {
                    return false;
                }
                var first = t.First();
                var flag1 = first == command || first.IsMatch(command);

                return flag1;
            });
        }
        public static bool IsMatch(this ICommand command, Type type)
        {
            return command.GetType() == type;
        }
        public static List<string[]> IdentifyVariables(this string code)
        {
            return IdentifyVariables(code.Split(' '));
        }
        public static List<string[]> IdentifyVariables(this string[] splitCode)
        {
            List<int> indices = new List<int>(splitCode.Length);
            for (int i = 0; i < splitCode.Length; i++)
            {
                var section = splitCode[i]; 
                
                if (_storage.FetchGCommand(section) != null || _storage.FetchMCommand(section) != null || _storage.FetchArbitraryCommand(section) != null)
                {
                    indices.Add(i);
                }
            }

            List<string[]> arrOfCommands = new List<string[]>(indices.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                int range;
                if (indices.Count > i + 1)
                {
                    range = indices[i + 1] - indices[i];
                }
                else
                {
                    range = splitCode.Length - indices[i];
                }
                
                var str = splitCode.Skip(indices[i]).Take(range).ToArray();
                arrOfCommands.Add(str);
            }

            return arrOfCommands;
        }
        public static List<ICommand> GatherCommands(this List<string[]> arrOfCommands)
        {
            List<ICommand> commands = new List<ICommand>();
            foreach (var commandString in arrOfCommands)
            {
                var id = commandString[0];
                ICommand command = _storage.TryGetCommand(id);
                for (int i = 1; i < commandString.Length; i++)
                {
                    var par = commandString[i];
                    command.Parameters.Values[par[0].ToString()] = float.Parse(new string(par.Skip(1).ToArray()));
                }

                commands.Add(command);
            }

            return commands;
        }
        
    }
}