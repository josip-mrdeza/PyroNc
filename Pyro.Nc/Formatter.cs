using System;
using System.Globalization;
using System.Text;
using Pyro.Math;

namespace Pyro.Nc
{
    public static class Formatter
    {
        private static readonly StringBuilder Builder = new StringBuilder();     
        private const string Qualifier = "{0}";
        public static string Format(this string template, object[] args)
        {
            lock (Builder)
            {
                Builder.Clear();
                Builder.Append(template);
                for (int i = 0; i < args.Length; i++)
                {
                    Builder.Replace($"{{{i}}}", args[i].ToString());
                }

                return Builder.ToString();
            }
        }
        /// <summary>
        /// Causes no allocation (apart from the returned string) formatting if the struct has an overriden ToString method.
        /// </summary>
        public static string Format<T>(this string template, T val)
        {
            lock (Builder)
            {
                Builder.Clear();
                Builder.Append(template);
                Builder.Replace(Qualifier, val.ToString());

                return Builder.ToString();
            }
        }
        
        /// <summary>
        /// Causes no allocation (apart from the returned string) formatting if the struct has an overriden ToString method.
        /// </summary>
        public static string Format<T1, T2>(this string template, T1 val, T2 val2)
        {
            lock (Builder)
            {
                Builder.Clear();
                Builder.Append(template);
                Builder.Replace(Qualifier, val.ToString());
                Builder.Replace("{1}", val2.ToString());
                return Builder.ToString();
            }
        }
        
        /// <summary>
        /// Causes no allocation (apart from the returned string) formatting if the struct has an overriden ToString method.
        /// </summary>
        public static string Format<T1, T2, T3>(this string template, T1 val, T2 val2, T3 val3)
        {
            lock (Builder)
            {
                Builder.Clear();
                Builder.Append(template);
                Builder.Replace("{0}", val.ToString());
                Builder.Replace("{1}", val2.ToString());
                Builder.Replace("{2}", val3.ToString());
                return Builder.ToString();
            }
        }
    }
}