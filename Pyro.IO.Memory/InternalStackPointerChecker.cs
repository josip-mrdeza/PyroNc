using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pyro.IO.Memory;

internal static class InternalStackPointerChecker
{
    internal static readonly Dictionary<IntPtr, List<EventHandler<IntPtr>>> Subscribers = new();
    internal static bool HasBeenInitialized;
    [Obsolete]
    internal static void Subscribe(IntPtr ptr, EventHandler<IntPtr> resolveSub)
    {
        if (Subscribers.ContainsKey(ptr))
        {
            Subscribers[ptr].Add(resolveSub);
        }
        else
        {
            Subscribers.Add(ptr, new List<EventHandler<IntPtr>>{resolveSub});
        }

        if (!HasBeenInitialized)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    //TODO!
                    Thread.Sleep(1);
                }
            });
        }
    }

    internal static void Unsubscribe(IntPtr ptr, EventHandler<IntPtr> handler)
    {
        if (Subscribers.ContainsKey(ptr))
        {
            Subscribers[ptr].Remove(handler);
        }
        else
        {
            throw new NotSupportedException("No such value exists in the dictionary!");
        }
    }

    internal static void UnsubscribeAllFor(IntPtr ptr)
    {
        if (Subscribers.ContainsKey(ptr))
        {
            Subscribers.Remove(ptr);
        }
        else
        {
            throw new NotSupportedException("No such value exists in the dictionary!");
        }
    }
}