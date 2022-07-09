using System;
using System.Collections.Generic;
using System.Linq;

namespace Pyro.IO
{
    public static class LINQExt
    {
        public static TR Mutate<T, TR>(this T obj, Func<T, TR> func) => func(obj);

        public static IEnumerable<T> For<T>(this IEnumerable<T> collection, Action<T, int> action, ref int i, int iterations)
        {
            var enumerator = collection.GetEnumerator();
            for (; i < iterations; i++)
            {
                enumerator.MoveNext();
                action(enumerator.Current, i);
            }
            enumerator.Dispose();

            return collection;
        }
        
        public static IEnumerable<T> For<T>(this IEnumerable<T> collection, Action<T, int> action, ref int i)
        {
            return collection.For(action, ref i, collection.Count());
        }

        public static IEnumerable<T> ForApplicable<T>(this IEnumerable<T> collection, Predicate<T> predicate, Action<T, int> action, ref int i)
        {
            T[] arr = collection as T[];
            
            if (arr is null)
            {
                var list = collection as List<T>;
                return list.For(action, ref i, list!.Count(x => predicate(x)));
            }

            arr.For(action, ref i, arr.Count(x => predicate(x)));
            return arr;
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
    }
}