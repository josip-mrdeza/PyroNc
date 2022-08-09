using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class ABaseCommand : ICommand
    {
        public ABaseCommand(ITool tool, ArbitraryCommandParameters parameters)
        {
            Tool = tool;
            Parameters = parameters;
        }

        public ITool Tool { get; set; }
        public bool IsModal { get; }
        public bool IsArc { get; }
        public virtual string Description { get; }
        public ICommandParameters Parameters { get; set; }

        public virtual async Task Execute()
        {
            await Execute(false);
        }

        public virtual async Task Execute(bool draw)
        {
            
        }

        public void Expire()
        {
            throw new System.NotImplementedException();
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