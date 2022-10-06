using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pyro.IO;
using Pyro.Math;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Parsing
{
    public static class CommandHelper
    {
        internal static ValueStorage Storage;
        internal static readonly List<int> CachedIndices = new List<int>(14);
        internal static readonly List<string[]> CachedArrOfCommands = new List<string[]>(7);
        internal static readonly List<ICommand> CachedCommands = new List<ICommand>(7);
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
                if (index >= 1 && code[index - 1] != ' ')
                {
                    code = code.Insert(index, " ");
                } 
            }
            return FindVariables(code.Split(' '));
        }
        public static List<string[]> FindVariables(this string[] splitCode)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < splitCode.Length; i++)
            {
                var section = splitCode[i].Trim(); 
                if (Storage.FetchArbitraryCommand(section) != null || Storage.FetchGCommand(section) != null || Storage.FetchMCommand(section) != null)
                {
                    indices.Add(i);
                }
                else
                {
                    for (int j = 0; j < Storage.ArbitraryCommands.Keys.Count; j++)
                    {
                        if (section.Contains(Storage.ArbitraryCommands.Keys.ElementAt(j)))
                        {
                            indices.Add(i);
                        }
                    }
                }
            }

            List<string[]> arrOfCommands = new List<string[]>();
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
            string[] commandString = null;
            string id = null;
            for (var index = 0; index < arrOfCommands.Count; index++)
            {
                try
                {
                    commandString = arrOfCommands[index];
                    id = commandString[0];
                    if (ScrapComment(arrOfCommands, id, index, commands, out var list))
                    {
                        return list;
                    }

                    if (ScrapNotation(id, commands) || ScrapToolChange(id, commands) ||
                        ScrapSpindleSpeedSetter(id, commands) || ScrapFeedRateSetter(id, commands))
                    {
                        continue;
                    }

                    var command = Storage.TryGetCommand(id);
                    if (command is null)
                    {
                        continue;
                    }

                    foreach (var key in command.Parameters.Values.Keys.ToArray())
                    {
                        command.Parameters.Values[key] = Single.NaN;
                    }

                    for (int i = 1; i < commandString.Length; i++)
                    {
                        var par = commandString[i];
                        var reqStr = new string(par.Skip(1).ToArray());
                        var success = float.TryParse(reqStr, out var f);
                        if (!success)
                        {
                            reqStr = reqStr.Replace("(", "").Replace(")", "");
                            var nums = reqStr.LookForNumbers();
                            var results = nums.Solve();
                            f = (float) results[1].Value;
                            Debug.Log($"Solver result: {f.ToString(CultureInfo.InvariantCulture)}");
                        }

                        command.Parameters.Values[par[0].ToString()] = f;
                    }

                    commands.Add(command);
                }
                catch (NullReferenceException e)
                {
                    PyroConsoleView.PushTextStatic(
                        "A NullReferenceException has occured in CommandHelper.CollectCommands:",
                        $"List length: {commands.Count.ToString()}",
                        $"List contents: [{string.Join(" ", commands)}]",
                        $"Current index: {index.ToString()}",
                        $"Current string: [{string.Join(" ", commandString!)}]",
                        $"Current id: {id}",
                        e.Message, e.TargetSite.Name);
                }
                catch (IndexOutOfRangeException e)
                {
                    PyroConsoleView.PushTextStatic(
                        "A IndexOutOfRangeException has occured in CommandHelper.CollectCommands:",
                        $"List length: {commands.Count.ToString()}",
                        $"List contents: [{string.Join(" ", commands)}]",
                        $"Current index: {index.ToString()}",
                        $"Current string: [{string.Join(" ", commandString!)}]",
                        $"Current id: {id}",
                        e.Message, e.TargetSite.Name);
                }
                catch (FormatException e)
                {
                    // PyroConsoleView.PushTextStatic(
                    //     "A FormatException has occured in CommandHelper.CollectCommands:",
                    //     $"Current id: {id}",
                    //     e.Message);
                }
                catch (Exception e)
                {
                    PyroConsoleView.PushTextStatic(
                        "An Exception has occured in CommandHelper.CollectCommands:",
                        $"List length: {commands.Count.ToString()}",
                        $"List contents: [{string.Join(" ", commands)}]",
                        $"Current index: {index.ToString()}",
                        $"Current string: [{string.Join(" ", commandString!)}]",
                        $"Current id: {id}",
                        e.Message, e.TargetSite.Name);
                }
            }

            return commands;
        }

        private static bool ScrapSpindleSpeedSetter(string id, List<ICommand> commands)
        {
            if (id[0] is 'S' or 's')
            {
                var secondPart = id.Split('S')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                } 
                var command = Storage.FetchArbitraryCommand("S");
                command.Parameters.AddValue("value", num);
                commands.Add(command);

                return true;
            }

            return false;
        }
        
        private static bool ScrapFeedRateSetter(string id, List<ICommand> commands)
        {
            if (id[0] is 'F' or 'f')
            {
                var secondPart = id.Split('F')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                } 
                var command = Storage.FetchArbitraryCommand("F");
                command.Parameters.AddValue("value", num);
                commands.Add(command);

                return true;
            }

            return false;
        }

        private static bool ScrapToolChange(string id, List<ICommand> commands)
        {
            if (id[0] is 'T' or 't')
            {
                var secondPart = id.Split('T')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                }
                var command = Storage.FetchArbitraryCommand("T");
                command.Parameters.AddValue("value", num);
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
            if (id[0] is 'N' or 'n')
            {
                var secondPart = id.Remove(0, 1);
                var isSecondPartLong = long.TryParse(secondPart, out long num);
                if (!isSecondPartLong)
                {
                    return false;
                }
                var notation = Storage.FetchArbitraryCommand("N") as Notation;
                notation!.Number = num;
                commands.Add(notation);

                return true;
            }

            return false;
        }

        internal static KeyValuePair<string, ICommand>? _cachedKvp;
    }
}