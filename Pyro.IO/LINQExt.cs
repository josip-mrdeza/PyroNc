using System;
using System.Collections.Generic;
using System.Linq;

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

        public static TR MutateCastRef<T, TR>(this T obj) where T : class
                                                          where TR : class
        {
            return (TR) (object) obj;
        }

        public static TR MutateCastNonRef<T, TR>(this T obj) where TR : T
        {
            return (TR) obj;
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
                    return i - substring.Length + 1;
                }
            }

            return -1;
        }
        
        public static int ContainsFast(this string s, char key)
        {
            if (s.Length <= 1)
            {
                return -1;
            }

            var length = s.Length;
            for (int i = 0; i < length; i++)
            {
                bool match = false;
                match = s[i] == key;

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        public static IEnumerable<string> SplitFast(this string s, string separator)
        {
            if (s.Length < separator.Length)
            {
                yield break;
            }

            var length = separator.Length * s.Length;
            var lastIndex = 0;
            for (int i = 0; i < length; i++)
            {
                bool match = false;
                foreach (var c in separator)
                {
                    match = s[i] == c;
                }

                if (match)
                {                                                               
                    yield return s.Substring(lastIndex, i - lastIndex);
                    lastIndex = i + 1;
                }
            }

            yield return s.Substring(lastIndex + 1, s.Length - lastIndex - 1);
        }
    }
}
