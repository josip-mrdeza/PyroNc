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
                await listener.OnCommandExecute(command);
            }
        }
    }

    public interface IListener
    {
        /// <summary>
        /// A method definition which is called on every class which inherits IListener once any command gets executed in their respective Execution queues.
        /// </summary>
        /// <returns>A task to be awaited.</returns>
        public Task OnCommandExecute(Command command);
    }
}
