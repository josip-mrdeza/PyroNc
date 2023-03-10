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
        internal static MathParser ExpParser = new MathParser("", VariableMap);
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
            if (string.IsNullOrEmpty(code))
            {
                return new List<string[]>();
            }
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
                    if (Regex.IsMatch(section, @"\w\([^)]+\)"))
                    {
                        continue;
                    }
                    else if (splitCode[0] == "DEF")
                    {
                        continue;
                    }
                    if (Regex.IsMatch(section, @"\$(.+)"))
                    {
                        continue; //problem with this taking S / F
                    }
                    for (int j = 0; j < Storage.ArbitraryCommands.Keys.Count; j++)
                    {
                        if (section.Contains(Storage.ArbitraryCommands.Keys.ElementAt(j)))
                        {
                            activated = true;
                            indices.Add(i);
                        }
                    }
                }   
                if ((Storage.FetchUnresolved(section) != null && !activated))
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
        
        public static List<BaseCommand> CollectCommands(this List<string[]> arrOfCommands)
        {
            arrOfCommands = arrOfCommands.Select(x =>
            {
                return x.Where(y =>
                {
                    return !string.IsNullOrEmpty(y);
                }).ToArray();
            }).Where(z => z.Length > 0).ToList();
            List<BaseCommand> commands = new List<BaseCommand>();
            string[] commandString = null;
            string id = null;
            Exception exception = null;
            int line = 0;
            for (var index = 0; index < arrOfCommands.Count; index++)
            {
                try
                {
                    commandString = arrOfCommands[index];
                    id = commandString[0];
                    if (ScrapComment(arrOfCommands, id, index, commands, out var list)) { return list; }
                    id = id.ToUpperInvariant();
                    if (ScrapVariableDefinition(string.Join(" ", commandString), commands))
                    {
                        continue;
                    }

                    if (ScrapDereferencesDeclaration(id, out var bc))
                    {
                        id = id.Replace($"${bc.Name}", bc.Value.ToString());
                        continue;
                    }

                    if (ScrapNotation(id, commands) || ScrapToolChange(id, commands) ||
                        ScrapSpindleSpeedSetter(id, commands) || ScrapFeedRateSetter(id, commands))
                    {
                        continue;
                    }
                    // if (SynCommandHelper.PopulateIfValid(id, ref commandString, commands))
                    // {
                    //     return commands;
                    // }
                    if (SynCommandHelper.HandleForLoopCondition(id, commandString, commands))
                    {
                        continue;
                    }
                    var command = (BaseCommand) Storage.TryGetCommand(id).Copy();
                    if (command is null) { continue; }
                    if (GetCycle(arrOfCommands, command, commands)) continue;
                    if (GetMCALL(arrOfCommands, command, index, commands, out list)) return list;
                    ResetCommandParameters(command);
                    PopulateCommandParameters(commandString, command);
                    command = FinalizeCommand(command, commandString);
                    commands.Add(command);
                }
                catch (NullReferenceException e)
                {
                    if (true || PreviousExceptionMessage != e.Message)
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
                    continue;
                }
            }
            Globals.Rules.Try(commands);
            if (exception is not null)
            {
                //fail
            }
            return commands;
        }

        private static bool ScrapDereferencesDeclaration(string id, out ValuePointerDereference command)
        {
            var match = Regex.Match(id, @"\$([\w]+|[\d]+)");
            if (match.Success)
            {
                var valueDerf = new ValuePointerDereference(Globals.Tool, new ArbitraryCommandParameters());
                valueDerf.Name = match.Groups[1].Value;
                command = valueDerf;
                return true;
            }

            command = null;
            return false;
        }
        private static bool ScrapVariableDefinition(string id, List<BaseCommand> commands)
        {
            bool partialOk = Regex.IsMatch(id, @"DEF");
            var match = Regex.Match(id, @"DEF (\w+) (\w+)\=(.+)");
            if (match.Success)  //regular
            {
                var varType = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var value = match.Groups[3].Value;
                var ok = Enum.TryParse<VariableType>(varType, out var type);
                if (!ok)
                {
                    throw new RuleParseException($"Invalid variable type: {varType}!");
                }
                DEF def = new DEF(type, false, name, DEF.CreateVariableOfType(type, false, value), 0, 0);
                commands.Add(def);
                def.Execute(false).RunSynchronously();
                return true;
            }

            match = Regex.Match(id, @"DEF (\w+)\[?(\d?)\]? (\w+)\[(\d+),?(\d?)\]");
            if (match.Success) //array
            {
                var varType = match.Groups[1].Value;
                int.TryParse(match.Groups[2].Value, out var n);
                var name = match.Groups[3].Value;
                int.TryParse(match.Groups[4].Value, out n);
                int.TryParse(match.Groups[5].Value, out var m);
                var ok = Enum.TryParse<VariableType>(varType, out var e);
                if (!ok)
                {
                    throw new RuleParseException($"Invalid variable type: {varType}!");
                }

                var variable = DEF.CreateVariableOfType(e, true, null, n, m);
                DEF def = new DEF(e, true, name, variable, n, m);
                commands.Add(def);
                def.Execute(false).RunSynchronously();
                return true;
            }

            match = Regex.Match(id, "DEF STRING\\[(\\d+)\\] (\\w+)\\=\"(.*)\"");
            if (match.Success) //string
            {
                var name = match.Groups[2].Value;
                var value = match.Groups[3].Value;
                DEF def = new DEF(VariableType.STRING, false, name, value, value.Length, 1);
                commands.Add(def);
                def.Execute(false).RunSynchronously();
                return true;
            }

            if (partialOk)
            {
                var def = new DEF(Globals.Tool, new ArbitraryCommandParameters());
                def.Description = "Partial variable declaration";
                commands.Add(def);
            }
            return partialOk;
        }

        private static BaseCommand FinalizeCommand(BaseCommand command, string[] commandString)
        {
            if (command is UnresolvedCommand unresolvedCommand)
            {
                command = FixUnresolvedCommand(commandString, command, unresolvedCommand);
            }
            else
            {
                SetModalCommand(command);
            }

            return command;
        }
        private static void SetModalCommand(BaseCommand command)
        {
            if (command.IsModal)
            {
                PreviousModal = (BaseCommand)command;
            }
        }
        private static BaseCommand FixUnresolvedCommand(string[] commandString, BaseCommand command,
            UnresolvedCommand unresolvedCommand)
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
                    if (ScrapDereferencesDeclaration(reqStr, out var drc))
                    {
                        if (drc.Value == null)
                        {
                            command.AdditionalInfo += $"\n[Cannot use variable {drc.Name} before it's declaration.]";
                            continue;
                        }
                        var d = (double)drc.Value;
                        f = (float)d;
                        reqStr = reqStr.Replace($"${drc.Name}", f.ToString());
                        command.AdditionalInfo += $"\n[Borrowing->{drc.Name}:{drc.Value}]";
                    }
                    reqStr = reqStr.Replace("(", "").Replace(")", "");
                    ExpParser.Expression = reqStr;
                    // var nums = reqStr.LookForNumbers();
                    // var results = nums.Solve();
                    f = (float) ExpParser.Evaluate();
                    ExpParser.Expression = null;
                }

                command.Parameters.Values[char.ToUpperInvariant(par[0]).ToString()] = f;
            }

            var lastModal = PreviousModal;
            if (lastModal == null)
            {
                throw new NoModalCommandFoundException(command.Description);
            }

            command = (BaseCommand) lastModal.Copy();
            command.AdditionalInfo = "(Modal)";
            command.Parameters = unresolvedCommand.Parameters;

            return command;
        }
        internal static void PopulateCommandParameters(string[] commandString, BaseCommand command, int charLen = 1)
        {
            for (int i = 1; i < commandString.Length; i++)
            {
                var par = commandString[i];
                var reqStr = new string(par.Skip(charLen).ToArray());
                if (string.IsNullOrEmpty(reqStr))
                {
                    continue;
                }

                var success = float.TryParse(reqStr, out var f);
                if (!success)
                {
                    if (ScrapDereferencesDeclaration(reqStr, out var drc))
                    {
                        if (drc.Value == null)
                        {
                            command.AdditionalInfo += $"\n[Cannot use variable {drc.Name} before it's declaration.]";
                            continue;
                        }
                        var d = (double)drc.Value;
                        f = (float)d;
                        reqStr = reqStr.Replace($"${drc.Name}", f.ToString());
                        command.AdditionalInfo += $"\n[Borrowing->{drc.Name}:{drc.Value}]";
                    }
                    reqStr = reqStr.Replace("(", "").Replace(")", "");
                    ExpParser.Expression = reqStr;
                    // var nums = reqStr.LookForNumbers();
                    // var results = nums.Solve();
                    f = (float) ExpParser.Evaluate();
                    ExpParser.Expression = null;
                }

                command.Parameters.Values[char.ToUpperInvariant(par[0]).ToString()] = f;
            }
        }
        private static void ResetCommandParameters(BaseCommand command)
        {
            foreach (var key in command.Parameters.Values.Keys.ToArray())
            {
                command.Parameters.Values[key] = Single.NaN;
            }
        }
        private static bool GetMCALL(List<string[]> arrOfCommands, BaseCommand command, int index, List<BaseCommand> commands, out List<BaseCommand> list)
        {
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
                        var spf = new SubProgramCall(mcall.ToolBase, mcall.Parameters);
                        spf.Name = cmd;
                        mcall.NextSubroutine = spf;
                    }
                }

                commands.Add(mcall);
                {
                    list = commands;

                    return true;
                }
            }

            list = null;
            return false;
        }
        private static bool GetCycle(List<string[]> arrOfCommands, BaseCommand command, List<BaseCommand> commands)
        {
            if (command is Cycle)
            {
                var fullString = string.Join("", arrOfCommands[arrOfCommands.FindIndex(x =>
                                                                   x.FirstOrDefault(
                                                                       y => y.ToUpperInvariant()
                                                                             .Contains("CYCLE")) !=
                                                                   null)]);
                var actualCycle = ExtractCycle(fullString);

                commands.Add(actualCycle);

                return true;
            }

            return false;
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
        private static bool ScrapSpindleSpeedSetter(string id, List<BaseCommand> commands)
        {
            if (id[0] is 'S' or 's')
            {
                var secondPart = id.Split('S')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                } 
                var command = (BaseCommand) Storage.FetchArbitraryCommand("S").Copy();
                command.Parameters.AddValue("value", num);
                commands.Add(command);

                return true;
            }

            return false;
        }
        private static bool ScrapFeedRateSetter(string id, List<BaseCommand> commands)
        {
            if (id[0] is 'F' or 'f')
            {
                var secondPart = id.Split('F')[1];
                var isSecondPartFloat = float.TryParse(secondPart, out float num);
                if (!isSecondPartFloat)
                {
                    return false;
                } 
                var command = (BaseCommand) Storage.FetchArbitraryCommand("F").Copy();
                command.Parameters.AddValue("value", num);
                commands.Add(command);

                return true;
            }

            return false;
        }
        private static bool ScrapToolChange(string id, List<BaseCommand> commands)
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
                var command = (BaseCommand) Storage.FetchArbitraryCommand("T").Copy();
                command.Parameters.AddValue("value", num);
                commands.Add(command);
                return true;
            }

            return false;
        }
        private static bool ScrapComment(List<string[]> arrOfCommands, string id, int index, List<BaseCommand> commands,
            out List<BaseCommand> list)
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
        private static bool ScrapNotation(string id, List<BaseCommand> commands)
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
        internal static KeyValuePair<string, BaseCommand>? _cachedKvp;
    }
}