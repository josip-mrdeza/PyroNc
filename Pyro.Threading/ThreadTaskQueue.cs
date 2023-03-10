using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pyro.Threading
{
    public class ThreadTaskQueue
    {
        public int Name
        {
            get;
            private set;
        }
        public TimeSpan DelayBetweenChecks
        {
            get;
            set;
        } = TimeSpan.FromMilliseconds(1d);

        public bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Name;
        public ThreadTaskQueue()
        {
            _queue = new ConcurrentQueue<ThreadDelegateStore>();
            Name = Thread.CurrentThread.ManagedThreadId;
            //_thread = new Thread(async () => await ThreadHandle());
            //_thread.IsBackground = true;
            //_thread.Start();
        }

        public void FireAndForget(Action a)
        {
            _queue.Enqueue(new ThreadDelegateStore(a, null));
        }

        public void FireAndForget<T>(Action<T> a, T val)
        {
            _queue.Enqueue(new ThreadDelegateStore(a, val));
        }

        public void FireAndForget<T>(EventHandler<T> eh, T val)
        {
            _queue.Enqueue(new ThreadDelegateStore(eh, val));
        }

        public async ValueTask Run<T>(Action<T> a, T val)
        {
            var tds = new ThreadDelegateStore(a, val);
            await InvokeOrEnqueue(tds);
        }
        
        public async ValueTask Run(Action a)
        {
            var tds = new ThreadDelegateStore(a, null);
            await InvokeOrEnqueue(tds);
        }

        public async ValueTask<TOut> Run<TOut>(Func<TOut> fn)
        {
            var tds = new ThreadDelegateStore(fn);
            await InvokeOrEnqueue(tds);
            return (TOut) tds.ComputedValue;
        }

        public async ValueTask<TOut> Run<T, TOut>(Func<T, TOut> fn, T val)
        {
            var tds = new ThreadDelegateStore(fn, val); 
            await InvokeOrEnqueue(tds);
            return (TOut) tds.ComputedValue;
        }
        
        public async ValueTask<TOut> Run<T, TOut>(Func<T, Task<TOut>> fn, T val)
        {
            var tds = new ThreadDelegateStore(fn, val);
            await InvokeOrEnqueue(tds);
            return (TOut) tds.ComputedValue;
        }
        
        public async ValueTask<TOut> Run<T, T2, TOut>(Func<T, T2, TOut> fn, T val1, T2 val2)
        {
            var tds = new ThreadDelegateStore(fn, val1, val2);
            await InvokeOrEnqueue(tds);

            return (TOut) tds.ComputedValue;
        }

        public async ValueTask Run<T>(Func<T, Task> fn, T val)
        {
            var tds = new ThreadDelegateStore(fn, val);
            await InvokeOrEnqueue(tds);
        }

        private readonly ConcurrentQueue<ThreadDelegateStore> _queue;
        //private readonly Thread _thread;
        //private readonly Func<object, object> _taskHandlerFunc;

        public async ValueTask ThreadHandle()
        {
            while (_queue.Count > 0)
            {
                var success = _queue.TryDequeue(out var store);
                if (success)
                {
                    InvokeDelegate(store);
                }
            }
        }

        private void InvokeDelegate(ThreadDelegateStore store, bool checkMain = true)
        {
            try
            {
                store.CompletionTask.Start();
                store.ComputedValue = store.Delegate.DynamicInvoke(store.Parameters);
                store.IsCompleted = true;
            }
            catch (Exception e)
            {
                store.Error = e;
            }
        }

        private async ValueTask InvokeOrEnqueue(ThreadDelegateStore store)
        {
            if (IsMainThread)
            {
                store.ComputedValue = store.Delegate.DynamicInvoke(store.Parameters);
                store.CompletionTask = Task.CompletedTask;
                store.IsCompleted = true;
            }
            else
            {
                _queue.Enqueue(store);
            }

            await store.CompletionTask;
        }
    }
}