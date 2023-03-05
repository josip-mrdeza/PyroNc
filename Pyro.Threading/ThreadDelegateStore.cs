using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pyro.Threading
{
    public class ThreadDelegateStore
    {
        public MulticastDelegate Delegate { get; }
        public object[] Parameters { get; }

        public Task CompletionTask { get; internal set; }
        public Exception Error { get; internal set; }
        public bool IsCompleted { get; internal set; }
        public object ComputedValue { get; internal set; }

        public ThreadDelegateStore(MulticastDelegate @delegate, params object[] parameters)
        {
            Delegate = @delegate;
            Parameters = parameters;
            CompletionTask = new Task(_taskAwaiter, this);
            Error = null; 
        }
        internal static void TaskHandler(ThreadDelegateStore tds)
        {
            var ts = TimeSpan.FromMilliseconds(0.1d);
            while (!tds.IsCompleted)
            {
                Thread.Sleep(ts);
            }
        }
        private readonly Action<object> _taskAwaiter = tds => TaskHandler((ThreadDelegateStore) tds);
    }
}