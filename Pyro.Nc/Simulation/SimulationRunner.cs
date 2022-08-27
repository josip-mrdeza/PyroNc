using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Parsing;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Simulation
{
    public class SimulationRunner
    {
        public IEnumerable<string> Lines { get; private set; }
        public Queue<Queue<ICommand>> CommandsToRun { get; set; } = new Queue<Queue<ICommand>>();
        public Queue<ICommand> CurrentSet { get; private set; }
        public ITool Tool { get; set; }

        public void Init(IEnumerable<string> lines)
        {
            Lines = lines;
            var listOfCommands = Lines.Select(x => x.FindVariables().CollectCommands()).ToArray();
            foreach (var commands in listOfCommands)
            {
                Queue<ICommand> queue = new Queue<ICommand>();
                foreach (var command in commands)
                {
                    queue.Enqueue(command);
                }
                CommandsToRun.Enqueue(queue);
            }

            CurrentSet = CommandsToRun.Dequeue();
        }

        public async Task<bool> Next(bool draw)
        {
            var isEmpty = CurrentSet.Count == 0;
            if (isEmpty)
            {
                if (CommandsToRun.Count == 0)
                {
                    return true;
                }
                CurrentSet = CommandsToRun.Dequeue();
                return false;
            }

            var nextCommand = CurrentSet.Dequeue();
            if (Tool is not null)
            {
                await Tool.UseCommand(nextCommand, draw);
            }
            else
            {
                await nextCommand.Execute(false);
            }

            return false;
        }

        public async Task RunUntilExhausted(bool draw)
        {
            while (!await Next(draw)) {}
        }
    }
}