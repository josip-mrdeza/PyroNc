using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pyro.Math
{
    public static class Solver
    {
        public struct SolverResult
        {
            private static StringBuilder Builder = new StringBuilder();
            public SolverResult(bool isNumber, double value)
            {
                IsNumber = isNumber;
                Value = value;
            }
            
            public bool IsNumber { get; set; }
            public double Value { get; set; }

            public static implicit operator SolverResult(double d)
            {
                return new SolverResult(true, d);
            }

            public override string ToString()
            {
                lock (Builder)
                {
                    Builder.Clear();
                    if (!IsNumber)
                    {
                        Builder.Append(System.Math.Abs(Value - 1) > 0.001d ? Value.ToString(CultureInfo.InvariantCulture) : string.Empty).Append('X');
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
                var sb = new StringBuilder(results.FirstOrDefault().ToString()).Append(" = ").Append((results.Count > 1 ? results[1] : default).ToString());

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
                if (builder[i] is '-' or '+' or '=')
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
                equation += "= 0";
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
            bool CaseX(string val, bool negative)
            {
                var altVal = val.ToLower();
                if (altVal.Contains('x'))
                {
                    var alt = altVal.Replace('x', ' ').TrimEnd();
                    
                    var sr = new SolverResult(false, alt is not "" ? double.Parse(alt) : 1);
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
            for (int i = 0; i < arr.Length; i++)
            {
                var value = arr[i];
                if (nextIsNegative)
                {
                    if (CaseX(value, true))
                    {
                        nextIsNegative = false;
                        continue;
                    }

                    buffer.Add(-double.Parse(value));
                    nextIsNegative = false;
                }
                else if (value == "-")
                {
                    nextIsNegative = true;
                }
                else if (value == "+")
                {
                    continue;
                }
                else
                {
                    if (CaseX(value, false))
                    {
                        continue;
                    }
                    buffer.Add(double.Parse(value));
                }
            }
        }
        public static List<SolverResult> Solve(this List<SolverResult>[] results)
        {
            List<SolverResult> solved = new List<SolverResult>();
            double x = 0;
            double rhs = 0;
            foreach (var result in results[0])
            {
                if (result.IsNumber)
                {
                    rhs -= result.Value;
                }
                else
                {
                    x += result.Value;
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
                    x -= result.Value;
                }
            }

            if (x != 0)
            {
                if (x < 0)
                {
                    rhs = -rhs;
                    x = -x;
                }

                if (x != 1)
                {
                    rhs /= x;
                    x = 1;
                }

                solved.Add(new SolverResult(false, x));
            }
            else
            {
                solved.Add(new SolverResult(true, 0));
            }

            solved.Add(new SolverResult(true, rhs));
            return solved;
        }
        public static SolverResultString ToResultString(this List<SolverResult> results) => results;
        public static double ParseNumber(this string s)
        {
            return double.Parse(s);
        } 
    }
}