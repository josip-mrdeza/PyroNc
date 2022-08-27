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
                if (code[index - 1] != ' ')
                {
                    code = code.Insert(index, " ");
                } 
            }
            return FindVariables(code.Split(' '));
        }
        public static List<string[]> FindVariables(this string[] splitCode)
        {
            List<int> indices = new List<int>(splitCode.Length);
            List<int> joinLater = new List<int>();
            for (int i = 0; i < splitCode.Length; i++)
            {
                var section = splitCode[i]; 
                if (_storage.FetchArbitraryCommand(section) != null || _storage.FetchGCommand(section) != null || _storage.FetchMCommand(section) != null)
                {
                    indices.Add(i);
                    continue;
                }
                else
                {
                    for (int j = 0; j < _storage.ArbitraryCommands.Keys.Count; j++)
                    {
                        if (section.Contains(_storage.ArbitraryCommands.Keys.ElementAt(j)))
                        {
                            indices.Add(i);
                            continue;
                        }
                    }
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
                var id = commandString[0];
                if (ScrapComment(arrOfCommands, id, index, commands, out var list))
                {
                    return list;
                }
                if (ScrapNotation(id, commands) || ScrapToolChange(id, commands) || ScrapSpindleSpeedSetter(id, commands))
                {
                    continue;
                }
                var command = _storage.TryGetCommand(id);
                for (int i = 1; i < commandString.Length; i++)
                {
                    var par = commandString[i];
                    command.Parameters.Values[par[0].ToString()] = float.Parse(new string(par.Skip(1).ToArray()));
                }

                commands.Add(command);
            }

            return commands;
        }

        private static bool ScrapSpindleSpeedSetter(string id, List<ICommand> commands)
        {
            if (id.Contains("S"))
            {
                var command = _storage.FetchArbitraryCommand("S");
                command.Parameters.AddValue("value", float.Parse(id.Split('S')[1]));
                commands.Add(command);

                return true;
            }

            return false;
        }

        private static bool ScrapToolChange(string id, List<ICommand> commands)
        {
            if (id.Contains("T"))
            {
                var command = _storage.FetchArbitraryCommand("T");
                command.Parameters.AddValue("value", float.Parse(id.Split('T')[1]));
                commands.Add(command);

                return true;
            }

            return false;
        }

        private static bool ScrapComment(List<string[]> arrOfCommands, string id, int index, List<ICommand> commands,
            out List<ICommand> list)
        {
            if (id.Contains(_cachedKvp!.Value.Key))
            {
                var comment = _cachedKvp!.Value.Value.Copy() as Comment;
                comment!.Text = string.Join(" ", arrOfCommands.Mutate(c =>
                {
                    return c.Skip(index).Select(ci => string.Join(" ", ci)).ToArray();
                }));
                commands.Add(comment);
                {
                    list = commands;

                    return true;
                }
            }

            list = null;
            return false;
        }

        private static bool ScrapNotation(string id, List<ICommand> commands)
        {
            if (id.Contains('N'))
            {
                var notation = _storage.FetchArbitraryCommand("N") as Notation;
                notation!.Number = long.Parse(id.Remove(0, 1));
                commands.Add(notation);

                return true;
            }

            return false;
        }

        internal static KeyValuePair<string, ICommand>? _cachedKvp;
    }
}