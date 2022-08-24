using System.Threading;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using TrCore;

namespace Pyro.Nc.Parsing
{
    public class BaseCommand : ICommand
    {
        public BaseCommand(ITool tool, ICommandParameters parameters)
        {
            Tool = tool.GuardNullVariable("BaseCommand<ctor>.Tool");
            Parameters = parameters.GuardNullVariable("BaseCommand<ctor>.Parameters");
        }
        public ITool Tool { get; set; }
        public virtual string Description { get; }
        public virtual bool IsModal { get; }
        public virtual bool IsArc { get; }
        public ICommandParameters Parameters { get; set; }
        public void UpdateCurrent() => Tool.Current = this;

        public async Task ExecuteFinal(bool draw)
        {
            UpdateCurrent();
            if (Tool.TokenSource.IsCancellationRequested)
            {
                Tool.TokenSource.Dispose();
                Tool.TokenSource = new CancellationTokenSource();
            }
            Parameters.Token = Tool.TokenSource.Token;
            await Execute(draw);
        }
        public virtual Task Execute(bool draw) => throw new System.NotImplementedException();
        public virtual void Expire() => throw new System.NotImplementedException();
        public virtual void Plan() => throw new System.NotImplementedException();
        public ICommand Copy() => MemberwiseClone() as ICommand;
    }
}