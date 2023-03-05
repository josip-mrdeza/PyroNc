using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pyro.Math
{
    public static class Solver
    {
        public enum Operation
        {
            Reserved = 0,
            Addition = -1, 
            Subtraction = -2,
            Multiplication = 1,
            Division = 2,
            Factorization = 4,
            Sin,
            Cos,
            Tan,
            Ctg,
            Modulo = 3,
            Default
        }
        public struct SolverResult
        {
            private static StringBuilder Builder = new StringBuilder();
            public SolverResult(bool isNumber, double value, Operation op = Operation.Default, string id = "")
            {
                IsNumber = isNumber;
                Value = value;
                Op = op;
                Id = id;
            }

            public bool IsNumber { get; set; }
            public double Value { get; set; }
            public string Id { get; set; }
            public Operation Op { get; set; }

            public void ChangeOp(Operation o)
            {
                Op = o;
            }

            public void ChangeValue(double val)
            {
                Value = val;
            }

            public static implicit operator SolverResult(double d)
            {
                return new SolverResult(true, d, default);
            }

            public override string ToString()
            {
                lock (Builder)
                {
                    Builder.Clear();
                    if (!IsNumber)
                    {
                        Builder.Append(System.Math.Abs(Value - 1) > 0.001d ? Value.ToString(CultureInfo.InvariantCulture) : string.Empty).Append(Id);
                    }
                    else
                    {
                        Builder.Append(Value);
                    }
                    return Builder.ToString();
                }
                
            }
        }
        public class SolverResultString
        {
            public SolverResultString(string result)
            {
                Result = result;
            }
            
            public string Result { get; set; }
            
            public override string ToString() => Result;

            public static implicit operator SolverResultString(List<SolverResult> results)
            {
                //var sb = new StringBuilder(results.FirstOrDefault().ToString()).Append(" = ").Append((results.Count > 1 ? results[1] : default).ToString());
                var sb = new StringBuilder();
                List<double> numbers = new();
                SolverResult last = default;
                foreach (var result in results)
                {
                    if (result.IsNumber)
                    {
                        numbers.Add(result.Value);
                    }
                    else
                    {
                        if (result.Value < 0)
                        {
                            sb.Append('-').Append(' ');
                        }

                        if (last.Id != null)
                        {
                            if (result.Value > 0)
                            {
                                sb.Append('+').Append(' ');
                            }
                        }
                        sb.Append(result.Value is 1 or -1 ? "" : result.Value.ToString(CultureInfo.InvariantCulture)).Append(result.Id).Append(' ');
                        last = result;
                    }
                }

                sb.Append('=').Append(' ');
                sb.Append(numbers.Sum());
                return new SolverResultString(sb.ToString());
            }
        }
        public static List<SolverResult>[] LookForNumbers(this string equation)
        {
            StringBuilder builder = new StringBuilder(new string(equation.ToCharArray().Where(x => x is not ' ' or '\0').ToArray()));
            List<SolverResult>[] pair = new List<SolverResult>[] {
                new List<SolverResult>(), new List<SolverResult>()
            };
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] is '-' or '+' or '=' or '*' or '/' or '%')
                {
                    if (i != 0)
                    {
                        builder.Insert(i, ' ').Insert(i + 2, ' ');
                        i += 2;
                    }
                    else
                    {
                        builder.Insert(i + 1, ' ');
                    }
                }
            }

            if (!equation.Contains("="))
            {
                if (!equation.ToLower().Contains("x"))
                {
                    builder.Insert(0, 'x').Insert(1, '=');
                }
                else
                {
                    builder.Append("=0");
                }
            }

            var str = builder.ToString();
            var complete = str.Split('=');
            var left = complete[0].TrimEnd();
            var right = complete[1].TrimStart();

            var leftArr = left.Split(' ');
            var rightArr = right.Split(' ');
            ResolveNumbersInEq(pair[0], leftArr);
            ResolveNumbersInEq(pair[1], rightArr);

            return pair;
        }
        public static void ResolveNumbersInEq(List<SolverResult> buffer, string[] arr)
        {
            bool CaseX(string val, bool negative, Operation operation)
            {
                var altVal = val.ToLower();
                char ch = Char.MinValue;
                if (altVal.Any(c =>
                {
                    var flag = char.IsLetter(c);
                    if (flag)
                    {
                        ch = c;
                    }

                    return flag;
                }))
                {
                    var alt = altVal.Replace(ch, ' ').TrimEnd();
                    
                    var sr = new SolverResult(false, alt is not "" ? double.Parse(alt) : 1, operation, ch.ToString());
                    if (negative)
                    {
                        sr.Value = -sr.Value;
                    }
                    buffer.Add(sr);

                    return true;
                }

                return false;
            }
            bool nextIsNegative = false;
            Operation currentOp = default;
            for (int i = 0; i < arr.Length; i++)
            {
                var value = arr[i].ToLowerInvariant();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                if (nextIsNegative)
                {
                    if (CaseX(value, true, currentOp))
                    {
                        currentOp = Operation.Default;
                        nextIsNegative = false;
                        continue;
                    }

                    buffer.Add(new SolverResult(true, -double.Parse(value), Operation.Subtraction));
                    currentOp = Operation.Default;
                    nextIsNegative = false;
                }
                else if (value == "-")
                {
                    currentOp = Operation.Subtraction;
                    nextIsNegative = true;
                }
                else if (value == "+")
                {
                    currentOp = Operation.Addition;
                }
                else if (value == "*")
                {
                    currentOp = Operation.Multiplication;
                }
                else if (value == "/")
                {
                    currentOp = Operation.Division;
                }
                else if (value == "%")
                {
                    currentOp = Operation.Modulo;
                }
                else if (value.StartsWith("sin"))
                {
                    value = value.Remove(0, 3);
                    buffer.Add(new SolverResult(true, float.Parse(value).Sin(), currentOp));
                }
                else if (value.StartsWith("cos"))
                {
                    value = value.Remove(0, 3);
                    buffer.Add(new SolverResult(true, float.Parse(value).Cos(), currentOp));
                }
                else if (value.StartsWith("tan"))
                {
                    value = value.Remove(0, 3);
                    buffer.Add(new SolverResult(true, float.Parse(value).Tan(), currentOp));
                }
                else if (value.StartsWith("ctg"))
                { 
                    value = value.Remove(0, 3);
                    var f = float.Parse(value);
                    buffer.Add(new SolverResult(true, f.Cos() / f.Sin(), currentOp));
                }
                else
                {
                    if (CaseX(value, false, currentOp))
                    {
                        continue;
                    }

                    if (value.EndsWith("!"))
                    {
                        currentOp = Operation.Factorization;
                        value = value.Remove(value.Length - 1);
                        buffer.Add(new SolverResult(true, ((int) double.Parse(value)).Factorial(), currentOp));
                    }
                    else
                    {
                        buffer.Add(new SolverResult(true, double.Parse(value), currentOp));
                    }
                }
            }
        }
        public static List<SolverResult> Solve(this List<SolverResult>[] results)
        {
            List<SolverResult> solved = new List<SolverResult>();
            Dictionary<string, double> dict = new Dictionary<string, double>();
            double rhs = 0;
            foreach (var result in results[0])
            {
                if (result.IsNumber)
                {
                    rhs -= result.Value;
                }
                else
                {
                    if (dict.ContainsKey(result.Id))
                    {
                        dict[result.Id] += result.Value;
                    }
                    else
                    {
                        dict.Add(result.Id, result.Value);
                    }
                }
            }
            double lastHigherOrderOpResult = 1f;
            for (var i = 0; i < results[1].Count; i++)
            {
                var arr = results[1];
                var result = arr[i];
                if (result.IsNumber)
                {
                    if (result.Op is Operation.Addition)
                    {
                        rhs = Addition(arr, ref lastHigherOrderOpResult, ref i, rhs, result);
                    }
                    else if (result.Op is Operation.Subtraction)
                    {
                        rhs = Subtraction(arr, ref lastHigherOrderOpResult, ref i, rhs, result);
                    }
                    else if (result.Op is Operation.Multiplication)
                    {
                        rhs = Multiplicate(arr, ref lastHigherOrderOpResult, result, ref i, ref rhs);
                    }
                    else if (result.Op is Operation.Division)
                    {
                        rhs = Division(arr, ref lastHigherOrderOpResult, result, ref i, rhs);
                    }
                    else if (result.Op is Operation.Modulo)
                    {
                        rhs = Modulo(arr, ref lastHigherOrderOpResult, result, ref i, rhs);
                    }        
                    else if (result.Op is Operation.Reserved)
                    {
                        
                    }
                    else
                    {
                        //rhs += result.Value;
                    }
                }
                else
                {
                    if (dict.ContainsKey(result.Id))
                    {
                        dict[result.Id] -= result.Value;
                    }
                    else
                    {
                        dict.Add(result.Id, -result.Value);
                    }
                }
            }

            foreach (var kvp in dict)
            {
                var x = kvp.Value;
                if (x != 0)
                {
                    if (x != -1 && System.Math.Abs(x - 1) > 0.000001d)
                    {
                        rhs /= x;
                        x = 1;
                    }

                    solved.Add(new SolverResult(false, x, id: kvp.Key));
                }
                else
                {
                    solved.Add(new SolverResult(true, 0));
                }
            }

            solved.Add(new SolverResult(true, rhs));
            return solved;
        }

        private static double Addition(List<SolverResult> results, ref double lastHigherOrderOp, ref int i, double rhs, SolverResult result)
        {
            bool higherOrder = results.Count - 1 >= i + 1 && results.IsNextHighOrder(i);
            if (higherOrder)
            {
                i++;
                Multiplicate(results, ref lastHigherOrderOp, results[i], ref i, ref rhs);
                i--;
                return rhs;
            }

            if (results.IsPreviousHighOrder(i))
            {
                rhs += result.Value;
            }
            else
            {
                rhs += result.Value;
                rhs += results[i - 1].Value;
            }
            lastHigherOrderOp = 1f;
            return rhs;
        }

        private static double Subtraction(List<SolverResult> results, ref double lastHigherOrderOp, ref int i, double rhs, SolverResult result)
        {
            if (!(results.Count -1 > i + 1))
            {
                return rhs;
            }

            rhs += results[i + 1].Value;
            rhs += result.Value;
            lastHigherOrderOp = 1f;
            i++;
            return rhs;
            
        }

        private static double Modulo(List<SolverResult> results, ref double lastHigherOrderOp, SolverResult result, ref int i, double rhs)
        {
            double res;
            if (lastHigherOrderOp == 1d)
            {
                res = result.Value % results[i + 1].Value;
                result.Value = res;
            }
            else
            {
                res = result.Value % lastHigherOrderOp;
                result.Value = res;
            }

            rhs += res;
            lastHigherOrderOp = res;
            i++;
            return rhs;
        }

        private static double Division(List<SolverResult> results, ref double lastHigherOrderOp, SolverResult result, ref int i, double rhs)
        {
            double res;
            if (lastHigherOrderOp == 1d)
            {
                res = results[i - 1].Value / result.Value;
                result.Value = res;
            }
            else
            {
                res = lastHigherOrderOp / result.Value;
                result.Value = res;
            }

            rhs += res;
            lastHigherOrderOp = res;
            i++;
            return rhs;
        }

        private static double Multiplicate(List<SolverResult> results, ref double lastHigherOrderOp, SolverResult result, ref int i,
            ref double rhs)
        {
            double res;
            if (lastHigherOrderOp == 1d)
            {
                res = result.Value * results[i - 1].Value;
                var sr = results[i - 1];
                sr.Value = res;
                results[i - 1] = sr;
            }
            else
            {
                res = result.Value * lastHigherOrderOp;
            }
            lastHigherOrderOp = res;
            if (results.IsNextOp(i, Operation.Multiplication))
            {
                i++;
                Multiplicate(results, ref lastHigherOrderOp, results[i], ref i, ref rhs);
                i++;
                return rhs;
            }
            rhs += res;
            return rhs;
        }

        public static List<SolverResult> SolveFor(this List<SolverResult>[] results, List<SolverResult> missingVariables)
        {
            List<SolverResult> solved = new List<SolverResult>();
            Dictionary<string, double> dict = new Dictionary<string, double>();
            Dictionary<string, double> dictMissing = missingVariables.ToDictionary(k => k.Id, k => k.Value);
            double rhs = 0;                                                                               
            foreach (var result in results[0])
            {
                if (result.IsNumber)
                {
                    rhs -= result.Value;
                }
                else
                {
                    if (dict.ContainsKey(result.Id))
                    {
                        dict[result.Id] += result.Value;
                    }
                    else
                    {
                        dict.Add(result.Id, result.Value);
                    }
                }
            }

            foreach (var result in results[1])
            {
                if (result.IsNumber)
                {
                    rhs += result.Value;
                }
                else
                {
                    if (dict.ContainsKey(result.Id))
                    {
                        dict[result.Id] -= result.Value;
                    }
                    else
                    {
                        dict.Add(result.Id, -result.Value);
                    }
                }
            }

            for (int i = 0; i < dict.Count; i++)
            {
                var kvp = dict.ElementAt(i);
                if (dictMissing.TryGetValue(kvp.Key, out var val))
                {
                    dict[kvp.Key] = kvp.Value * val;
                }
            }

            foreach (var kvp in dict)
            {
                var x = kvp.Value;
                if (x != 0)
                {
                    if (x < 0)
                    {
                        rhs = -rhs;
                        x = -x;
                    }
                    if (rhs == 0 && missingVariables.Exists(y => y.Id == kvp.Key))
                    {
                        solved.Add(new SolverResult(true, x, id: kvp.Key));
                        continue;
                    }
                    if (System.Math.Abs(x - 1) > 0.000001d)
                    {
                        rhs /= x;
                        x = 1;
                    }
                    
                    solved.Add(new SolverResult(false, x, id: kvp.Key));
                }
                else
                {
                    solved.Add(new SolverResult(true, 0));
                }
            }

            solved.Add(new SolverResult(true, rhs));
            return Solve(new []{new List<SolverResult>()
            {
                new SolverResult(false, 1, default, "x")
            }, solved});
        }

        public static List<SolverResult> MergeWith(this List<SolverResult>[] results, List<SolverResult>[] otherResults)
        {
            var otherSolved = otherResults.Solve();
            var solved = results.SolveFor(otherSolved);

            return solved;
        }
        public static SolverResultString ToResultString(this List<SolverResult> results) => results;
        public static double ParseNumber(this string s)
        {
            return double.Parse(s);
        }

        public static long Factorial(this int num)
        {
            long result = 1;
            for (int i = 1; i < num + 1; i++)
            {
                result *= i;
            }

            return result;
        }

        public static bool IsNextOp(this List<SolverResult> list, int currentIndex, Operation op)
        {
            if (!list.IsNextAvailable(currentIndex))
            {
                return false;
            }

            var next = list[currentIndex + 1];

            return next.Op == op;
        }

        public static bool IsPreviousOp(this List<SolverResult> list, int currentIndex, Operation op)
        {
            if (!list.IsPreviousAvailable(currentIndex))
            {
                return false;
            }

            var previous = list[currentIndex - 1];

            return previous.Op == op;
        }

        public static bool IsPreviousHighOrder(this List<SolverResult> list, int currentIndex)
        {
            if (!list.IsPreviousAvailable(currentIndex))
            {
                return false;
            }

            var previous = list[currentIndex - 1];

            return (int)previous.Op >= 0;
        }

        public static bool IsNextHighOrder(this List<SolverResult> list, int currentIndex)
        {
            if (!list.IsNextAvailable(currentIndex))
            {
                return false;
            }

            var previous = list[currentIndex + 1];

            return (int) previous.Op >= 0;
        }

        public static bool IsNextAvailable(this List<SolverResult> list, int currentIndex)
        {
            return list.Count - 1 >= currentIndex + 1;
        }

        public static bool IsPreviousAvailable(this List<SolverResult> list, int currentIndex)
        {
            return currentIndex - 1 >= 0;
        }

        public static char GetOperatorSymbol(this Operation operation)
        {
            return operation switch
            {
                Operation.Addition       => '+',
                Operation.Subtraction    => '-',
                Operation.Multiplication => '*',
                Operation.Division       => '/', 
                Operation.Modulo         => '%',
                Operation.Factorization  => '!',
                _                        => '+'
            };
        }

        public static char GetOperatorSymbol(this int op)
        {
            return ((Operation)op).GetOperatorSymbol();
        }
        
        public static char GetOperatorSymbol(this byte op)
        {
            return ((Operation)(int) op).GetOperatorSymbol();
        }
    }
}