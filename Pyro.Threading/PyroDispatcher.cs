using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace Pyro.Threading
{
    public static class PyroDispatcher
    {
        private static Context Main => Context.DefaultContext;

        public static Task<TOut> ExecuteOnMainAsync<TIn, TOut>(Func<TIn, TOut> function, TIn input)
        {
            TOut result = default;
            Task<TOut> task = new Task<TOut>(i => ExecuteOnMain(function, (TIn) i), input);
            return task;
        }                                   
        public static TOut ExecuteOnMain<TIn, TOut>(Func<TIn, TOut> function, TIn input)
        {
            TOut result = default;
            Main.DoCallBack(() =>
            {
                result = function(input);
            });

            return result;
        }
        public static Task ExecuteOnMainAsync(Action function)
        {
            Task task = new Task(() => Main.DoCallBack(new CrossContextDelegate(function)));
            return task;
        }  
        public static Task ExecuteOnMainAsync<TIn>(Action<TIn> function, TIn input)
        {
            Task task = new Task(i => ExecuteOnMain(function, input), input);
            return task;
        }     
        public static void ExecuteOnMain<TIn>(Action<TIn> function, TIn input)
        {
            Main.DoCallBack(() => function(input)); 
        }
        
        public static TOut ExecuteOnMain<TOut>(Func<TOut> function)
        {
            TOut res = default;
            Main.DoCallBack(() =>
            {
                res = function();
            });

            return res;
        }

        public static void ExecuteOnMain(CrossContextDelegate function)
        {
            Main.DoCallBack(function);
        }
    }
}