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
using Pyro.Nc.Simulation;
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
        public void UpdateCurrent() => Tool.Values.Current = this;

        public async Task ExecuteFinal(bool draw)
        {
            var console = Globals.Console;
            var type = GetType().Name;
            var toDraw = draw.ToString();
            UpdateCurrent();
            if (Tool.Values.IsImperial)
            {
                Parameters.SwitchToImperial();
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Switched units to the imperial standard.");
            }

            if (Tool.Values.TokenSource.IsCancellationRequested)
            {
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Cancellation Requested!\n   --Cancelling operation...");
                Tool.Values.TokenSource.Dispose();
                Tool.Values.TokenSource = new CancellationTokenSource();
            }
            Parameters.Token = Tool.Values.TokenSource.Token;
            if (Tool.Values.IsMilling)
            {
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Executing in Mill mode...");
                await Execute(draw);
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Finished execution!");
            }
            else
            {
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Executing in Turn mode...");
                if(Parameters.GetValue("Y") != 0)
                {
                    console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Parameters contained a 'Y' value, which is forbidden in Turn mode!\n    --Throwing!!");
                    throw new NotSupportedException("Y axis is not supported in TURN Mode");
                }
                await ExecuteTurning(draw);
                console.PushText($"{type}: ExecuteFinal({toDraw})\n   --Finished execution!");
            }
        }
        public virtual Task Execute(bool draw)
        {
            var console = Globals.Console;
            var type = GetType().Name;
            var toDraw = draw.ToString();
            console.PushText($"{type}: Execute({toDraw})\n   --This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }
        public virtual Task ExecuteTurning(bool draw) => Execute(draw);

        public virtual void Expire()
        {
            var console = Globals.Console;
            var type = GetType().Name;
            console.PushText($"{type}: Expire()\n   --This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }

        public virtual void Plan()
        {
            var console = Globals.Console;
            var type = GetType().Name;
            console.PushText($"{type}: Plan()\n   --This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }

        public ICommand Copy()
        {
            var console = Globals.Console;
            var type = GetType().Name;
            console.PushText($"{type}: Copy()\n   --Copying ICommand::{type}!");
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
