using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyro.IO
{
    public static class LINQExt
    {
        public static IEnumerable<float> Max<T>(this IEnumerable<T> source, params Func<T, float>[] functions)
        {
            using var e = source.GetEnumerator();
            var shared = ArrayPool<float>.Shared;
            var arr = shared.Rent(functions.Length);
            var comparer = Comparer<float>.Default;
            while (e.MoveNext())
            {
                T nextValue = e.Current;
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i];
                    float nextKey = func(nextValue);
                    var curKey = arr[i];
                    if (comparer.Compare(nextKey, curKey) > 0)
                    {
                        arr[i] = nextKey;
                    }
                }
            }

            for (var i = 0; i < functions.Length; i++)
            {
                var val = arr[i];

                yield return val;
            }

            shared.Return(arr);
        }

        public static void Iterate(this IEnumerable enumerable)
        {
            var en = enumerable.GetEnumerator();
            while (en.MoveNext())
            {
                
            }
        }

        public static async Task IterateAsync<T>(this IAsyncEnumerable<T> source)
        {
            var e = source.GetAsyncEnumerator();
            while (await e.MoveNextAsync())
            {
                
            }
        }
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
