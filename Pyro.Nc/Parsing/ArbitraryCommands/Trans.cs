using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Trans : ICommand
    {
        public Trans(ITool tool, ArbitraryCommandParameters parameters)
        {
            Tool = tool;
            Parameters = parameters;
        }
        public ITool Tool { get; set; }
        public bool IsModal { get; }
        public bool IsArc { get; }
        public string Description { get; }
        public ICommandParameters Parameters { get; set; }

        public async Task Execute()
        {
            await Execute(false);
        }

        public async Task Execute(bool draw)
        {
            
        }

        public void Expire()
        {
            throw new System.NotSupportedException();
        }

        public void Plan()
        {
            throw new System.NotSupportedException();
        }

        public ICommand Copy()
        {
            return MemberwiseClone() as ICommand;
        }
    }
}