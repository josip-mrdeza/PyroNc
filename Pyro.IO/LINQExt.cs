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
    }
    
}
