using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyro.IO
{
    public static class LINQExt
    {
        public static TR Mutate<T, TR>(this T obj, Func<T, TR> func) => func(obj);
        public static TR Do<T, TR>(this T obj, Func<T, TR> func) => func(obj);
        public static void Do<T>(this T obj, Action<T> action) => action(obj);
        public static IEnumerable<TR> MutateCollection<T, TR>(this IEnumerable<T> collection, Func<T, TR> func)
        {
           return collection.Select<T, TR>(func);
        }
        public static T[] For<T>(this T[] collection, Action<T, int> action, int iterations, ref int i)
        {
            for (; i < iterations; i++)
            {
                action(collection[i], i);
            }

            return collection;
        }
        public static T[] For<T>(this T[] collection, Action<T, int> action, int iterations)
        {
            var i = 0;

            return collection.For(action, iterations, ref i);
        }
        public static T[] For<T>(this T[] collection, Action<T, int> action)
        {
            var i = 0;
            return collection.For(action, collection.Length, ref i);
        }
        public static T[] For<T>(this T[] collection, Action<T> action)
        {
            for (int i = 0; i < collection.Length; i++)
            {
                action(collection[i]);
            }

            return collection;   
        }
        public static T[] ForApplicable<T>(this T[] collection, Predicate<T> predicate, Action<T, int> action, ref int i)
        {
            collection.For(action, collection.Count(x => predicate(x)), ref i);
            return collection;
        }
        public static TR MutateCastRef<T, TR>(this T obj) where T : class where TR : class
        {
            return (TR) (object) obj;
        }
        public static TR MutateCastNonRef<T, TR>(this T obj) where TR : T
        {
            return (TR) obj;
        }

        public static IEnumerable<TR> SelectNoAlloc<T, TR>(this IEnumerable<T> collection, Func<T, TR> selector)
        {
            using var enumerator = collection.GetEnumerator();
            T current;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;

                yield return selector(current);
            }
        }
        public static IEnumerable<int> ContainsFastIndices(this string s, string substring)
        {
            if (s.Length < substring.Length)
            {
                yield break;
            }

            var length = substring.Length * s.Length;
            for (int i = 0; i < length; i++)
            {
                bool match = false;
                foreach (var c in substring)
                {
                    match = s[i] == c;
                }

                if (match)
                {
                    yield return i - substring.Length + 1;
                }
            }
        }
        public static int ContainsFast(this string s, string substring)
        {
            if (s.Length < substring.Length)
            {
                return -1;
            }

            var length = substring.Length * s.Length;
            for (int i = 0; i < length; i++)
            {
                bool match = false;
                foreach (var c in substring)
                {
                    match = s[i] == c;
                }

                if (match)
                {
                    return (i - substring.Length) + 1;
                }
            }

            return -1;
        }

        public static IEnumerable<string> SplitNoAlloc(this string text, char separator)
        {
            lock (StringBuilderSplitter)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char current = text[i];
                    if (current != separator)
                    {
                        StringBuilderSplitter.Append(char.ToUpperInvariant(current));
                    }
                    else
                    {
                        yield return StringBuilderSplitter.ToString();
                        StringBuilderSplitter.Clear();
                    }
                }

                yield return StringBuilderSplitter.ToString();
                StringBuilderSplitter.Clear();
            }
        }

        public static IEnumerable<string> SplitNoAlloc(this string text, string separator)
        {
            lock (StringBuilderStringSplitter)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    bool toContinue = true;
                    foreach (var c in separator)
                    {
                        if (text[i] != c)
                        {
                            StringBuilderStringSplitter.Append(text[i]);
                            toContinue = false;
                        }
                        if (!toContinue)
                        {
                            break;
                        }
                    }

                    if (!toContinue)
                    {
                        yield return StringBuilderStringSplitter.ToString();
                        StringBuilderStringSplitter.Clear();
                    }
                }
            }
        }
        
        private static readonly StringBuilder StringBuilderSplitter = new StringBuilder();
        private static readonly StringBuilder StringBuilderStringSplitter = new StringBuilder();
    }
    
}
