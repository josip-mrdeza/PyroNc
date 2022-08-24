using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Pathing;

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
            return code.FindVariables().CollectCommands().IsTrue(t =>
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
        public static List<string[]> FindVariables(this string code)
        {
            var index = code.ContainsFast(_cachedKvp!.Value.Key);
            if (index != -1)
            {
                code = code.Insert(index, " "); 
            }
            return FindVariables(code.Split(' '));
        }
        public static List<string[]> FindVariables(this string[] splitCode)
        {
            List<int> indices = new List<int>(splitCode.Length);
            for (int i = 0; i < splitCode.Length; i++)
            {
                var section = splitCode[i]; 
                
                if (_storage.FetchArbitraryCommand(section) != null || _storage.FetchGCommand(section) != null || _storage.FetchMCommand(section) != null)
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
        public static List<ICommand> CollectCommands(this List<string[]> arrOfCommands)
        {
            List<ICommand> commands = new List<ICommand>();
            for (var index = 0; index < arrOfCommands.Count; index++)
            {
                var commandString = arrOfCommands[index];
                ICommand command;
                var id = commandString[0];
                var cachedVal = _cachedKvp.Value;
                if (id.Contains(cachedVal.Key))
                {
                    var comment = cachedVal.Value.Copy() as Comment;
                    comment!.Text = string.Join(" ", arrOfCommands.Mutate(c =>
                    {
                        return c.Skip(index).Select(ci => string.Join(" ", ci)).ToArray();
                    }));
                    commands.Add(comment);
                    return commands;
                }

                command = _storage.TryGetCommand(id);
                for (int i = 1; i < commandString.Length; i++)
                {
                    var par = commandString[i];
                    command.Parameters.Values[par[0].ToString()] = float.Parse(new string(par.Skip(1).ToArray()));
                }

                commands.Add(command);
            }

            return commands;
        }

        internal static KeyValuePair<string, ICommand>? _cachedKvp;
    }
}