using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
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
            var type = GetType().Name;
            var toDraw = draw.ToString();
            await Tool.UntilValid();
            UpdateCurrent();
            if (InteropManager.RichPresence is not null)
            {
                var clientType = InteropManager.RichPresence.GetType();
                var clientInfo = clientType.GetField("Client");
                var client = clientInfo.GetValue(InteropManager.RichPresence);
            
                clientType = client.GetType();
                var method = clientType.GetMethod("UpdateDetails");
                method.Invoke(client, new object[]{$"{type}: '{Description}'"});
            }
            if (Tool.Values.IsImperial)
            {
                Parameters.SwitchToImperial();
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw})",
                                               "Switched units to the imperial standard.");
            }

            if (Tool.Values.TokenSource.IsCancellationRequested)
            {
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw})",
                                               "Cancellation Requested!",
                                               "Cancelling operation...");
                Tool.Values.TokenSource.Dispose();
                Tool.Values.TokenSource = new CancellationTokenSource();
            }
            Parameters.Token = Tool.Values.TokenSource.Token;
            if (Tool.Values.IsMilling)
            {
                List<string> msgs = new()
                {
                    $"{type}: ExecuteFinal({toDraw})",
                    $"CircleSmoothness: {Parameters.CircleSmoothness}",
                    $"LineSmoothness: {Parameters.LineSmoothness}",
                };
                msgs.AddRange(Parameters.Values.Select(x => x.ToString()));
                msgs.Add("Executing in Mill mode...");
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                await Execute(draw);
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw})",
                                               "Finished execution!");
            }
            else
            {
                List<string> msgs = new()
                {
                    $"{type}: ExecuteFinal({toDraw})",
                    $"CircleSmoothness: {Parameters.CircleSmoothness}",
                    $"LineSmoothness: {Parameters.LineSmoothness}",
                    string.Join(",\n", Parameters.Values),
                    "Executing in Turn mode..."
                };
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                if(Parameters.GetValue("Y") != 0)
                {
                    PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw})",
                                                   $"Parameters contained a 'Y' value, which is forbidden in Turn mode!",
                                                   "Throwing!!");
                    throw new NotSupportedException("Y axis is not supported in TURN Mode");
                }
                await ExecuteTurning(draw);
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw})",
                                               "Finished execution!");
            }
        }
        public virtual Task Execute(bool draw)
        {
            var type = GetType().Name;
            var toDraw = draw.ToString();
            PyroConsoleView.PushTextStatic($"{type}: Execute({toDraw})",
                                           $"This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }
        public virtual Task ExecuteTurning(bool draw) => Execute(draw);

        public virtual void Expire()
        {
            var type = GetType().Name;
            PyroConsoleView.PushTextStatic($"{type}: Expire()",
                                           $"This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }

        public virtual void Plan()
        {
            var type = GetType().Name;
            PyroConsoleView.PushTextStatic($"{type}: Plan()",
                                           $"This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }

        public ICommand Copy()
        {
            //var type = GetType().Name;
            //PyroConsoleView.PushTextStatic($"{type}: Copy()", "Copying ICommand::{type}!");
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
