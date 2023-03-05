using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pyro.Threading
{
    public class MainThread
    {
        public static MainThread Instance => GetMainThread();
        private static MainThread _instance;
        private readonly Lazy<ConcurrentQueue<ExecutionContext>> _functionQueueLazy;
        private ConcurrentQueue<ExecutionContext> FunctionQueue => _functionQueueLazy.Value;
        private readonly Lazy<Task> _constThread;
        private MainThread()
        {
            if (_instance != null)
            {
                throw new Exception("Another instance of MainThread has already been initialized, it is unclear how this constructor got invoked again. Use MainThread.GetMainThread() or MainThread.Instance to access the singleton.");
            }
            _instance = this;
            _functionQueueLazy = new Lazy<ConcurrentQueue<ExecutionContext>>(true);
            _constThread = new Lazy<Task>(Handler, true);
        }

        private async Task Handler()
        {
            var ts = TimeSpan.FromMilliseconds(1d);
            while (true)
            {
                if (FunctionQueue.Count > 0)
                {
                    while (FunctionQueue.TryDequeue(out var ec))
                    {
                        object[] arr = null;
                        if (ec.Value != null)
                        {
                            arr = ArrayPool<object>.Shared.Rent(1);
                            arr[0] = ec.Value;
                        }

                        var obj = ec.Delegate.DynamicInvoke(arr);
                        ec.ReturnValue = obj;
                        arr[0] = null;
                        ArrayPool<object>.Shared.Return(arr);
                    }
                }
                Thread.Sleep(ts);
            }
        }

        private void InitThread()
        {
            if (!_constThread.IsValueCreated)
            {
                var thread = _constThread.Value;
                thread.Start();
            }
        }

        public void Execute<T>(EventHandler<T> eh, T val)
        {
            InitThread();
            var ec = new ExecutionContext(eh, val);
            FunctionQueue.Enqueue(ec);
        }
        public void Execute(Action dg)
        {
            InitThread();
            FunctionQueue.Enqueue(dg);
        }

        public void Execute<T>(Action<T> dg, T val)
        {
            InitThread();
            FunctionQueue.Enqueue(new ExecutionContext(dg, val));
        }

        public ExecutionContext Execute<T>(Func<T> dg)
        {
            InitThread();
            var ec = new ExecutionContext(dg, null);
            FunctionQueue.Enqueue(ec);
            return ec;
        }
        public static MainThread GetMainThread()
        {
            if (_instance == null)
            {
                _instance = new MainThread();
            }

            return _instance;
        }

        public class ExecutionContext
        {
            internal readonly MulticastDelegate Delegate;
            internal readonly object Value;
            public object ReturnValue { get; internal set; }
            public event Action<object> OnCompleted;

            private void InvokeOnCompleted()
            {
                OnCompleted?.Invoke(ReturnValue);
            }

            public ExecutionContext(MulticastDelegate @delegate, object value)
            {
                Delegate = @delegate;
                Value = value;
                if (@delegate.GetType() == typeof(Func<>))
                {
                    throw new NotImplementedException();
                }
            }

            public static implicit operator ExecutionContext(MulticastDelegate dg)
            {
                return new ExecutionContext(dg, null);
            }
        }
    }
}