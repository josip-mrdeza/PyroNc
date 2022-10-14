using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using TrCore;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Parsing
{
    /// <summary>
    /// The base class for all ICommands
    /// </summary>
    public class BaseCommand : ICommand
    {
        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="tool">The tool used.</param>
        /// <param name="parameters">The parameters of the current ICommand.</param>
        /// <param name="throwOnNull">If the runtime should throw an error if the tool/parameters are null.</param>
        /// <param name="family">A family of commands.</param>
        public BaseCommand(ITool tool, ICommandParameters parameters, bool throwOnNull = false, Group family = Group.None)
        {
            Tool = tool.GuardNullVariable("BaseCommand<ctor>.Tool", throwOnNull);
            Parameters = parameters.GuardNullVariable("BaseCommand<ctor>.Parameters", throwOnNull);
            Family = family;
            Id = Guid.NewGuid();
        }
        /// <summary>
        /// The tool used.
        /// </summary>
        public ITool Tool { get; set; }

        public Group Family { get; set; }

        /// <summary>
        /// The command's unique description.
        /// </summary>
        public virtual string Description { get; }
        /// <summary>
        /// Defines whether the command can be stored as modular, meaning is it reusable.
        /// </summary>
        public virtual bool IsModal { get; }
        /// <summary>
        /// Defines whether the command is of arc movement type.
        /// </summary>
        public virtual bool IsArc { get; }

        public Guid Id { get; }
        /// <summary>
        /// Defines all parameters passed to the command from the parser (GCodeInputHandler->CommandHelper).
        /// </summary>
        public ICommandParameters Parameters { get; set; }
        /// <summary>
        /// Updates the tool's current command.
        /// </summary>
        public void UpdateCurrent()
        {
            Tool.Values.Current = this;
            Tool.Values.IsAllowed = false;
        }
        /// <summary>
        /// A final execution of the command, logging every step of the way and executing <see cref="ICommand.Execute"/> defined on the class inheriting <see cref="BaseCommand"/>.
        /// </summary>
        /// <param name="draw"></param>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/>
        /// does not define it's own <see cref="Execute"/> method, meaning it would do nothing.This is not allowed.</exception>
        /// <exception cref="NotSupportedException">This exception is thrown when the <see cref="ICommandParameters"/> of a <see cref="ExecuteTurning"/> method contain a 'Y' value.
        /// This is not allowed because the 'Y' value is not used in TURN Mode.</exception>
        public async Task ExecuteFinal(bool draw)
        {
            var type = GetType().Name;
            var toDraw = draw.ToString();
            await Tool.WaitUntilActionIsValid();
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
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                               "Switched units to the imperial standard.");
            }
            //Cancel();
            if (Tool.Values.IsMilling)
            {
                List<string> msgs = new()
                {
                    $"{type}: ExecuteFinal({toDraw}) - {Id}",
                    $"CircleSmoothness: {Parameters.CircleSmoothness}",
                    $"LineSmoothness: {Parameters.LineSmoothness}",
                };
                msgs.AddRange(Parameters.Values.Select(x => x.ToString()));
                msgs.Add("Executing in Mill mode...");
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                try
                {
                    await Execute(draw);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{type}: An error has occured in Execute -> {e.Message}!");
                    //PyroConsoleView.PushTextStatic($"{type}: An error has occured in Execute -> {e.Message}!");
                }
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                               "Finished execution!");
            }
            else
            {
                List<string> msgs = new()
                {
                    $"{type}: ExecuteFinal({toDraw}) - {Id}",
                    $"CircleSmoothness: {Parameters.CircleSmoothness}",
                    $"LineSmoothness: {Parameters.LineSmoothness}",
                    string.Join(",\n", Parameters.Values),
                    "Executing in Turn mode..."
                };
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                if(Parameters.GetValue("Y") != 0)
                {
                    PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                                   $"Parameters contained a 'Y' value, which is forbidden in Turn mode!",
                                                   "Throwing!!");
                    throw new NotSupportedException("Y axis is not supported in TURN Mode");
                }
                try
                {
                    await ExecuteTurning(draw);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{type}: An error has occured in ExecuteTurning -> {e.Message}!");
                    //PyroConsoleView.PushTextStatic($"{type}: An error has occured in ExecuteTurning -> {e.Message}!");
                }
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                               "Finished execution!");
            }
            Expire();
        }

        public void Cancel()
        {
            PyroConsoleView.PushTextStatic($"{GetType().Name}: ExecuteFinal(??) - {Id}",
                                           "Cancellation Requested!",
                                           "Cancelling operation...");
            Expire();
            Tool.Values.TokenSource.Cancel();
        }

        public void Renew()
        {
            Tool.Values.TokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// A method defining what a <see cref="ICommand"/> inheriting <see cref="BaseCommand"/> should do when executed in MILL Mode.
        /// </summary>
        /// <param name="draw">Defines whether to draw the path of the command or not.</param>
        /// <returns>A task.</returns>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="Execute"/> method.This is not allowed.</exception>
        public virtual Task Execute(bool draw)
        {
            var type = GetType().Name;
            var toDraw = draw.ToString();
            PyroConsoleView.PushTextStatic($"{type}: Execute({toDraw})",
                                           $"This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// A method defining what a <see cref="ICommand"/> inheriting <see cref="BaseCommand"/> should do when executed in TURN Mode.
        /// </summary>
        /// <param name="draw">Defines whether to draw the path of the command or not.</param>
        /// <returns>A task.</returns>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="ExecuteTurning"/> or <see cref="Execute"/> method.This is not allowed.</exception>
        public virtual Task ExecuteTurning(bool draw) => Execute(draw);

        /// <summary>
        /// A method that tells the <see cref="ITool"/> that the current command is expired and finished it's execution.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="Expire"/> method.This is not allowed.</exception>
        public void Expire()
        {
            Tool.Values.IsAllowed = true;
            Tool.Values.Current = null;
        }
        /// <summary>
        /// A method that tells the <see cref="ITool"/> which path it should define for the current <see cref="ICommand"/>.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="Plan"/> method.This is not allowed.</exception>
        public virtual void Plan()
        {
            var type = GetType().Name;
            PyroConsoleView.PushTextStatic($"{type}: Plan()",
                                           $"This method is not defined/overriden on the specific type of '{type}'!");
            throw new System.NotImplementedException();
        }
        public float ResolveNan(float val, float defaultVal)
        {
            if (float.IsNaN(val))
            {
                return defaultVal;
            }

            return val;
        }
        /// <summary>
        /// Copies the current <see cref="ICommand"/> and creates a new <see cref="ICommand"/> with the same values but a different address in memory.
        /// </summary>
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

            var instance = Activator.CreateInstance(this.GetType(), new object[]
            {
                Tool,
                parameters
            }) as ICommand;
            instance.Family = Family;

            return instance;
        }
    }
}
