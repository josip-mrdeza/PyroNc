using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using TrCore;

namespace Pyro.Nc.Parsing
{
    public class BaseCommand : ICommand
    {
        public BaseCommand(ITool tool, ICommandParameters parameters, bool throwOnNull = false)
        {
            Tool = tool.GuardNullVariable("BaseCommand<ctor>.Tool", throwOnNull);
            Parameters = parameters.GuardNullVariable("BaseCommand<ctor>.Parameters", throwOnNull);
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
            if (Tool.IsImperial)
            {
                Parameters.SwitchToImperial();
            }
            else
            {
                Parameters.SwitchToMetric();
            }
            
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

        public ICommand Copy()
        {
            var parameters = Parameters.GetType().Name switch
            {
                "GCommandParameters"         => new GCommandParameters(0, 0, 0, Parameters.LineSmoothness),
                "MCommandParameters"         => new MCommandParameters(),
                "ArbitraryCommandParameters" => new ArbitraryCommandParameters(),
                _                            => null as ICommandParameters
            };
            parameters.Values = Parameters.Values?.ToDictionary(k => k.Key, v => v.Value);

            return Activator.CreateInstance(this.GetType(), new object[]
            {
                Tool,
                parameters
            }) as ICommand;
        }
    }
}