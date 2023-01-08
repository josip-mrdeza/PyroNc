using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pyro.IO;
using Pyro.IO.PyroSc.Keywords;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.SyntacticalCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Parsing
{
    public static class CommandHelper
    {
        internal static ValueStorage Storage;
        internal static BaseCommand PreviousModal;
        internal static Dictionary<string, object> VariableMap = new Dictionary<string, object>();
        internal static string PreviousExceptionMessage;
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
            var index = code.IndexOf(_cachedKvp!.Value.Key, StringComparison.Ordinal);
            if (index != -1)
            {
                if (index >= 1 && code[index - 1] != ' ')
                {
                    code = code.Insert(index, " ");
                } 
            }

            if (code.StartsWith("FOR"))
            {
                return new List<string[]>()
                {
                    new string[]{code}
                };
            }
            return FindVariables(code.Split(' '));
        }
        public static List<string[]> FindVariables(this string[] splitCode)
        {
            List<int> indices = new List<int>();
            bool activated = false;
            for (int i = 0; i < splitCode.Length; i++)
            {
                var section = splitCode[i].Trim(); 
                Globals.Rules.Try(section);
                if (Storage.FetchGCommand(section) != null 
                    || Storage.FetchMCommand(section) != null
                    || Storage.FetchArbitraryCommand(section) != null 
                    || section.StartsWith("cycle", StringComparison.InvariantCultureIgnoreCase))
                {
                    activated = true;
                    indices.Add(i);

                    continue;
                }
                else
                {
                    for (int j = 0; j < Storage.ArbitraryCommands.Keys.Count; j++)
                    {
                        if (section.Contains(Storage.ArbitraryCommands.Keys.ElementAt(j)))
                        {
                            activated = true;
                            indices.Add(i);
                        }
                    }
                }   
                if (Storage.FetchUnresolved(section) != null && !activated)
                {
                    indices.Add(i);
                    activated = true;
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
            arrOfCommands = arrOfCommands.Select(x => x.Where(y => !string.IsNullOrEmpty(y)).ToArray()).Where(z => z.Length > 0).ToList();
            List<ICommand> commands = new List<ICommand>();
            string[] commandString = null;
            string id = null;
            Exception exception = null;
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

                    id = id.ToUpperInvariant();
                    if (ScrapNotation(id, commands) || ScrapToolChange(id, commands) ||
                        ScrapSpindleSpeedSetter(id, commands) || ScrapFeedRateSetter(id, commands))
                    {
                        continue;
                    }

                    ICommand command;
                    if (id.StartsWith("FOR"))
                    {
                        var reg = Regex.Matches(id, @"[\d]+");
                        var reg2 = Regex.Match(id, @"[\w]+=");
                        var r = reg.GetEnumerator();
                        int i = 0;
                        if (r.MoveNext())
                        {
                            int.TryParse(((Match) r.Current).Value, out i);
                        }
                        int iterations = 0;
                        if (r.MoveNext())
                        {
                            int.TryParse(((Match) r.Current).Value, out iterations);
                        }
                        r.Reset();
                        command = new ForLoopGCode(i, iterations, reg2.Value.Replace("=", ""));
                        commands.Add(command);
                        var loop = command as ForLoopGCode;
                        if (VariableMap.ContainsKey(loop.VariableName))
                        {
                            VariableMap[loop.VariableName] = loop.StartIndex;
                        }
                        else
                        {
                            VariableMap.Add(loop.VariableName, i);
                        }
                        return commands;
                    }
                    if (id.StartsWith("ENDFOR"))
                    {
                        command = new EndForGCode();
                        commands.Add(command);
                        return commands;
                    }
                    command = Storage.TryGetCommand(id).Copy();
                    if (command is null)
                    {
                        continue;
                    }

                    if (command is Cycle)
                    {
                        var fullString = string.Join("", arrOfCommands[arrOfCommands.FindIndex(x =>
                                                                           x.FirstOrDefault(
                                                                               y => y.ToUpperInvariant()
                                                                                   .Contains("CYCLE")) != null)]);
                        var actualCycle = ExtractCycle(fullString);

                        commands.Add(actualCycle);
                        continue;
                    }

                    if (command is MCALL mcall)
                    {
                        if (arrOfCommands.Count == 2)
                        {
                            var cmd = arrOfCommands[index + 1][0];
                            if (Regex.IsMatch(cmd, @"(CYCLE)(\d+)"))
                            {
                                mcall.NextSubroutine = ExtractCycle(cmd);
                            }
                            else //GOTTA BE AN SPF
                            {
                                var spf = new SubProgramCall(mcall.Tool, mcall.Parameters);
                                spf.Name = cmd;
                                mcall.NextSubroutine = spf;
                            }
                        }

                        commands.Add(mcall);
                        return commands;
                    }

                    foreach (var key in command.Parameters.Values.Keys.ToArray())
                    {
                        command.Parameters.Values[key] = Single.NaN;
                    }

                    for (int i = 1; i < commandString.Length; i++)
                    {
                        var par = commandString[i];
                        var reqStr = new string(par.Skip(1).ToArray());
                        if (string.IsNullOrEmpty(reqStr))
                        {
                            continue;
                        }

                        var success = float.TryParse(reqStr, out var f);
                        if (!success)
                        {
                            reqStr = reqStr.Replace("(", "").Replace(")", "");
                            var nums = reqStr.LookForNumbers();
                            var results = nums.Solve();
                            f = (float) results[1].Value;
                        }

                        command.Parameters.Values[char.ToUpperInvariant(par[0]).ToString()] = f;
                    }

                    if (command is UnresolvedCommand unresolvedCommand)
                    {
                        for (int i = 0; i < commandString.Length; i++)
                        {
                            var par = commandString[i];
                            var reqStr = new string(par.Skip(1).ToArray());
                            if (string.IsNullOrEmpty(reqStr))
                            {
                                continue;
                            }

                            var success = float.TryParse(reqStr, out var f);
                            if (!success)
                            {
                                reqStr = reqStr.Replace("(", "").Replace(")", "");
                                var nums = reqStr.LookForNumbers();
                                var results = nums.Solve();
                                f = (float) results[1].Value;

                                //Debug.Log($"Solver result: {f.ToString(CultureInfo.InvariantCulture)}");
                            }

                            command.Parameters.Values[char.ToUpperInvariant(par[0]).ToString()] = f;
                        }

                        var lastModal = PreviousModal;
                        if (lastModal == null)
                        {
                            throw new NoModalCommandFoundException(command.Description);
                        }

                        command = lastModal.Copy();
                        command.AdditionalInfo = "(Modal)";
                        command.Parameters = unresolvedCommand.Parameters;
                    }
                    else
                    {
                        if (command.IsModal)
                        {
                            PreviousModal = (BaseCommand) command;
                        }
                    }
                    commands.Add(command);
                }
                catch (NullReferenceException e)
                {
                    if (PreviousExceptionMessage != e.Message)
                    {
                        PyroConsoleView.PushTextStatic(
                            "An Exception has occured in CommandHelper.CollectCommands:",
                            $"List length: {commands.Count.ToString()}",
                            $"List contents: [{string.Join(" ", commands)}]",
                            $"Current index: {index.ToString()}",
                            $"Current string: [{string.Join(" ", commandString!)}]",
                            $"Current id: {id}",
                            e.Message, e.TargetSite.Name);
                        PreviousExceptionMessage = e.Message;
                    }
                }
                catch (NoModalCommandFoundException e)
                {
                    exception = e;
                }
                catch (Exception e)
                {
                    if (PreviousExceptionMessage != e.Message)
                    {
                        PyroConsoleView.PushTextStatic(
                            "An Exception has occured in CommandHelper.CollectCommands:",
                            $"List length: {commands.Count.ToString()}",
                            $"List contents: [{string.Join(" ", commands)}]",
                            $"Current index: {index.ToString()}",
                            $"Current string: [{string.Join(" ", commandString!)}]",
                            $"Current id: {id}",
                            e.Message, e.TargetSite.Name);
                        PreviousExceptionMessage = e.Message;
                    }
                }
            }

            Globals.Rules.Try(commands);

            if (exception is not null)
            {
                throw exception;
            }
            
            return commands;
        }

        private static Cycle ExtractCycle(string fullString)
        {
            StringBuilder builder = new StringBuilder(fullString);

            var parameterListString = builder.Remove(0, fullString.IndexOf('(') + 1).Replace(")", "").ToString();
            builder.Clear();
            for (int i = 0; i < parameterListString.Length; i++)
            {
                char c = parameterListString[i];
                if (c == ',')
                {
                    char leading = parameterListString.ElementAtOrDefault(i + 1);
                    builder.Append(' ');
                    if (leading == ',')
                    {
                        builder.Append("SKIP");
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }

            var name = fullString.Substring(0, fullString.IndexOf('(')).ToUpperInvariant();
            string parameters = builder.ToString();
            float[] splitParameters = parameters.Split(' ').Select(x =>
            {
                var b = float.TryParse(x, out var f);

                return b ? f : float.NaN;
            }).ToArray();
            var fullTypeName = "Pyro.Nc.Parsing.Cycles.{0}".Format(name);
            var type = Type.GetType(fullTypeName);
            Cycle actualCycle =
                Activator.CreateInstance(type, Globals.Tool, new ArbitraryCommandParameters(), splitParameters) as Cycle;

            return actualCycle;
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
                var command = Storage.FetchArbitraryCommand("S").Copy();
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
                var command = Storage.FetchArbitraryCommand("F").Copy();
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
                id = id.ToUpper();
                var secondPart = id.Split('T')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                }
                var command = Storage.FetchArbitraryCommand("T").Copy();
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
                })).Remove(0, 1);
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
                var notation = Storage.FetchArbitraryCommand("N").Copy() as Notation;
                notation!.Number = num;
                commands.Add(notation);

                return true;
            }

            return false;
        }

        internal static KeyValuePair<string, ICommand>? _cachedKvp;
    }
}