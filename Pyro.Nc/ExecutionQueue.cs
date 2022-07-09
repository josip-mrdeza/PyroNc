using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyro.Nc
{
    public class ExecutionQueue
    {
        public Queue<Command> CommandQueue = new Queue<Command>();
        public List<IListener> Listeners = new List<IListener>();

        public ExecutionQueue()
        {
        }

        public ExecutionQueue(IEnumerable<IListener> listeners)
        {
            Listeners.AddRange(listeners);
        }

        public ExecutionQueue(IListener listener)
        {
            Listeners.Add(listener);
        }

        public async Task Notify(Command command)
        {
            foreach (var listener in Listeners)
            {
                listener.OnCommandExecute(command);
            }
        }
    }

    public interface IListener
    {
        public void OnCommandExecute(Command command);
    }
}
