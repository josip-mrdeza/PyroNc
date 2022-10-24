using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pyro.IO.Events
{
    public class PyroEventSystem
    {
        public Dictionary<string, List<IPEventSubscriber>> Subscribers = new Dictionary<string, List<IPEventSubscriber>>();
        public Dictionary<string, List<IPAsyncEventSubscriber>> AsyncSubscribers = new Dictionary<string, List<IPAsyncEventSubscriber>>();

        public void AddSubscriber(string eventName, IPEventSubscriber subscriber)
        {  
            Lazy<string> subType = new Lazy<string>(() => subscriber.GetType().Name);
            if (subscriber == null)
            {
                return;
            }
            if (Subscribers.ContainsKey(eventName))
            {
                Subscribers[eventName].Add(subscriber);
            }
            else if (Subscribers.Values.FirstOrDefault(t => t.Exists(p => p.GetType().Name == subType.Value)) != null)
            {
                return;
            }
            else
            {
                Subscribers.Add(eventName, new List<IPEventSubscriber>(){subscriber});
            }
        }
        
        public void AddAsyncSubscriber(string eventName, IPAsyncEventSubscriber subscriber)
        {
            Lazy<string> subType = new Lazy<string>(() => subscriber.GetType().Name);
            if (subscriber == null)
            {
                return;
            }
            if (AsyncSubscribers.ContainsKey(eventName))
            {
                AsyncSubscribers[eventName].Add(subscriber);
            }
            else if (AsyncSubscribers.Values.FirstOrDefault(t => t.Exists(p => p.GetType().Name == subType.Value)) != null)
            {
                return;
            }
            else
            {
                AsyncSubscribers.Add(eventName, new List<IPAsyncEventSubscriber>(){subscriber});
            }
        }
        
        public void Fire(string eventName)
        {
            var exists = Subscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in Subscribers[eventName])
                {
                    subscriber.OnEventInvoked();                                     
                }
            }
        }
        
        public void Fire<T>(string eventName, T obj)
        {
            var exists = Subscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in Subscribers[eventName].Where(x => x is IPEventSubscriber<T>).Cast<IPEventSubscriber<T>>().ToArray())
                {
                    subscriber.OnEventInvoked(obj);
                }
            }
        }
        
        public void Fire<T, T2>(string eventName, T obj, T2 obj2)
        {
            var exists = Subscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in Subscribers[eventName].Where(x => x is IPEventSubscriber<T, T2>).Cast<IPEventSubscriber<T, T2>>().ToArray())
                {
                    subscriber.OnEventInvoked(obj, obj2);
                }
            }
        }
        
        public void Fire<T, T2, T3>(string eventName, T obj, T2 obj2, T3 obj3)
        {
            var exists = Subscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in Subscribers[eventName].Where(x => x is IPEventSubscriber<T, T2, T3>).Cast<IPEventSubscriber<T, T2, T3>>().ToArray())
                {
                    subscriber.OnEventInvoked(obj, obj2, obj3);
                }
            }
        }

        public async Task FireAsync(string eventName)
        {
            var exists = AsyncSubscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in AsyncSubscribers[eventName])
                {
                    await subscriber.OnEventInvoked();
                }
            }
        }
        
        public async Task FireAsync<T>(string eventName, T obj)
        {
            var exists = AsyncSubscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in AsyncSubscribers[eventName].Where(x => x is IPAsyncEventSubscriber<T>).Cast<IPAsyncEventSubscriber<T>>().ToArray())
                {
                    await subscriber.OnEventInvoked(obj);
                }
            }
        }
        
        public async Task FireAsync<T, T2>(string eventName, T obj, T2 obj2)
        {
            var exists = AsyncSubscribers.ContainsKey(eventName);
            if (exists)
            {
                foreach (var subscriber in AsyncSubscribers[eventName].Where(x => x is IPAsyncEventSubscriber<T, T2>).Cast<IPAsyncEventSubscriber<T, T2>>().ToArray())
                {
                    await subscriber.OnEventInvoked(obj, obj2);
                }
            }
        }
        
        public async Task FireAsync<T, T2, T3>(string eventName, T obj, T2 obj2, T3 obj3)
        {
            var exists = AsyncSubscribers.ContainsKey(eventName);
            if (exists)
            {
                var arr = AsyncSubscribers[eventName].Where(x => x is IPAsyncEventSubscriber<T, T2, T3>);
                foreach (var subscriber in arr.Cast<IPAsyncEventSubscriber<T, T2, T3>>().ToArray())
                {
                    await subscriber.OnEventInvoked(obj, obj2, obj3);
                }
            }
        }
        
        
    }
}