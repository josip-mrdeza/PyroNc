using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Parsing.ArbitraryCommands;
using UnityEngine;

namespace Pyro.Nc.Configuration;

public class PDispatcher : MonoBehaviour
{
    private static ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();
    private static ConcurrentQueue<DispatchAwaiter> ReturnQueue = new ConcurrentQueue<DispatchAwaiter>();
    private static TimeSpan TimeOut = TimeSpan.FromTicks(500);
    private static bool IsInitialized;
    private void Start()
    {
        IsInitialized = true;
    }

    public static void ExecuteOnMain(Action function)
    {
        Queue.Enqueue(function);
    }

    public static T ExecuteOnMain<T>(Func<object> function)
    {
        if (!IsInitialized)
        {
            return default;
        }
        DispatchAwaiter awaiter = new DispatchAwaiter();
        awaiter.Function = function;
        ReturnQueue.Enqueue(awaiter);
        while (!awaiter.IsComplete)
        {
            Thread.Sleep(TimeOut);
        }

        if (awaiter.Data != null)
        {
            return awaiter.Data.CastInto<T>();
        }

        return default;
    }

    private void LateUpdate()
    {
        while (Queue.TryDequeue(out var obj))
        {
            obj.Invoke();
        }

        while (ReturnQueue.TryDequeue(out var awaiter))
        {
            var obj = awaiter.Function.Invoke();
            awaiter.MarkComplete(obj);
        }
    }

    private class DispatchAwaiter
    {
        public object Data;
        public bool IsComplete;
        internal Func<object> Function;

        public void MarkComplete(object data)
        {
            Data = data;
            IsComplete = true;
        }
    }
}