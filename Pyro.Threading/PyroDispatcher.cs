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
        public static TOut ExecuteOnMain<TIn, TOut>(Func<TIn, TOut> function, TIn input)
        {
            TOut result = default;
            Main.DoCallBack(() =>
            {
                result = function(input);
            });

            return result;
        }

        public static void ExecuteOnMain<TIn>(Action<TIn> function, TIn input)
        {
            Main.DoCallBack(() => function(input)); 
        }

        public static void ExecuteOnMain(CrossContextDelegate function)
        {
            Main.DoCallBack(function);
        }
    }
}