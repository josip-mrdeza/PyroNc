using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pyro.Nc.Parsing
{
    /// <summary>
    /// The base class for all ICommands
    /// </summary>
    [Serializable]   
    public class BaseCommand : MachineComponent, ICommand
    {
        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="toolBase">The tool used.</param>
        /// <param name="parameters">The parameters of the current ICommand.</param>
        /// <param name="throwOnNull">If the runtime should throw an error if the tool/parameters are null.</param>
        /// <param name="family">A family of commands.</param>
        public BaseCommand(ToolBase toolBase, ICommandParameters parameters, bool throwOnNull = false, Group family = Group.None)
        {
            ToolBase = toolBase;
            Parameters = parameters;
            Family = family;
            Id = Guid.NewGuid();
            var fieldInfo = typeof(Locals).GetField(GetType().Name);
            if (fieldInfo is null)
            {
                AdditionalInfo = "";
                Description = "";
                return;
            }

            Description = (string) fieldInfo.GetValue(null);
        }
        /// <summary>
        /// The tool used.
        /// </summary>
        public ToolBase ToolBase { get; set; }

        public Group Family { get; set; }

        /// <summary>
        /// The command's unique description.
        /// </summary>
        public virtual string Description { get; internal set; }
        
        /// <summary>
        /// Defines whether the command can be stored as modular, meaning is it reusable.
        /// </summary>
        public virtual bool IsModal { get; }
        /// <summary>
        /// Defines whether the command is of arc movement type.
        /// </summary>
        public virtual bool IsArc { get; }
        public string AdditionalInfo { get; set; }
        public Guid Id { get; }
        /// <summary>
        /// Defines all parameters passed to the command from the parser (GCodeInputHandler->CommandHelper).
        /// </summary>
        public ICommandParameters Parameters { get; set; }
        public bool Is2DSimulation { get; set; }

        public int Line { get; set; } = -1;

        /// <summary>
        /// A final execution of the command, logging every step of the way and executing <see cref="ICommand.Execute"/> defined on the class inheriting <see cref="BaseCommand"/>.
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="skipSetup"></param>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/>
        /// does not define it's own <see cref="Execute"/> method, meaning it would do nothing.This is not allowed.</exception>
        /// <exception cref="NotSupportedException">This exception is thrown when the <see cref="ICommandParameters"/> of a <see cref="ExecuteTurning"/> method contain a 'Y' value.
        /// This is not allowed because the 'Y' value is not used in TURN Mode.</exception>
        public async Task ExecuteFinal(bool draw, bool skipSetup = false)
        {
            var type = GetType().Name;
            var toDraw = draw.ToString();
            if (InteropManager.RichPresence is not null)
            {
                var clientType = InteropManager.RichPresence.GetType();
                var clientInfo = clientType.GetField("Client");
                var client = clientInfo.GetValue(InteropManager.RichPresence);
            
                clientType = client.GetType();
                var method = clientType.GetMethod("UpdateDetails");
                method.Invoke(client, new object[]{$"[{type}]: '{Description}'"});
            }
            if (MachineBase.CurrentMachine.SimControl.Unit == UnitType.Imperial)
            {
                Parameters.SwitchToImperial();
                PyroConsoleView.PushTextStatic($"[{type}]:", "Switched units to the imperial standard.");
            }
            //Cancel();
            if (Is2DSimulation)
            {
                try
                {
                    Execute2D();
                }
                catch (Exception e)
                {
                    Globals.Console.Push($"~[{type}]: An error has occured in Execute -> {e.Message}!~");
                    throw;
                }
                PyroConsoleView.PushTextStatic($"[{type}]:", "Finished execution!");
                Expire();

                return;
            }
            if (MachineBase.CurrentMachine.CncType == MachineType.Mill || MachineBase.CurrentMachine.CncType == MachineType.Undefined)
            {
                List<string> msgs = new()
                {
                    $"{type} - {Description}"
                };
                msgs.AddRange(Parameters.Values.Where(y => !float.IsNaN(y.Value)).Select(x => x.ToString()));
                msgs.Add("Executing in Mill mode...");
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                if (skipSetup)
                {  
                    await Execute(draw);
                    PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                                   "Finished execution!");
                    return;
                }
                try
                {
                    await Execute(draw);
                }
                catch (Exception e)
                {
                    Globals.Console.Push($"{type}: An error has occured in Execute -> {e.Message}!");
                    throw;
                }
                PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                               "Finished execution!");
            }
            else
            {
                List<string> msgs = new()
                {
                    $"{type} - {Description}"
                };
                msgs.AddRange(Parameters.Values.Where(y => !float.IsNaN(y.Value)).Select(x => x.ToString()));
                msgs.Add("Executing in Turn mode...");
                PyroConsoleView.PushTextStatic(msgs.ToArray());
                if(Parameters.GetValue("Y") != 0)
                {
                    PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                                   $"Parameters contained a 'Y' value, which is forbidden in Turn mode!",
                                                   "Throwing!!");
                    throw new NotSupportedException("Y axis is not supported in TURN Mode");
                }

                if (skipSetup)
                {
                    await ExecuteTurning(draw);
                    PyroConsoleView.PushTextStatic($"{type}: ExecuteFinal({toDraw}) - {Id}",
                                                   "Finished execution!");
                    return;
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
                PyroConsoleView.PushTextStatic($"{type} - {Description} - Finished");
            }
            Expire();
        }

        public void Cancel()
        {
            PyroConsoleView.PushTextStatic($"{GetType().Name}: ExecuteFinal(??) - {Id}",
                                           "Cancellation Requested!",
                                           "Cancelling operation...");
            Expire();
            ToolBase.Values.TokenSource.Cancel();
        }

        public void Renew()
        {
            ToolBase.Values.TokenSource = new CancellationTokenSource();
        }

        public void Mark2DSimulation()
        {
            Is2DSimulation = true;
        }

        public void Mark3DSimulation()
        {
            Is2DSimulation = false;
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
            throw NotifyException.CreateNotifySystemException<NotImplementedException>(this, $"{type}: Execute({toDraw})" +
                $"\nThis method is not defined/overriden on the specific type of '{type}'!");
        }
        /// <summary>
        /// A method defining what a <see cref="ICommand"/> inheriting <see cref="BaseCommand"/> should do when executed in TURN Mode.
        /// </summary>
        /// <param name="draw">Defines whether to draw the path of the command or not.</param>
        /// <returns>A task.</returns>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="ExecuteTurning"/> or <see cref="Execute"/> method.This is not allowed.</exception>
        public virtual Task ExecuteTurning(bool draw) => Execute(draw);

        public virtual void Execute2D()
        {
            Globals.Console.PushComment($"{this.GetType().Name} does not implement {nameof(Execute2D)}, using ordinary execute for 3d sim instead.", Color.yellow);
        }

        /// <summary>
        /// A method that tells the <see cref="ToolBase"/> that the current command is expired and finished it's execution.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="Expire"/> method.This is not allowed.</exception>
        public void Expire()
        {
            MachineBase.CurrentMachine.StateControl.FreeControl();
            Mark3DSimulation();
        }
        /// <summary>
        /// A method that tells the <see cref="ToolBase"/> which path it should define for the current <see cref="ICommand"/>.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is thrown when the <see cref="ICommand"/> inheriting from <see cref="BaseCommand"/> does not implement it's own
        /// <see cref="Plan"/> method.This is not allowed.</exception>
        public virtual void Plan()
        {
            var type = GetType().Name;
            throw NotifyException.CreateNotifySystemException<NotImplementedException>(this, $"{type}: Plan()" +
                $"\nThis method is not defined/overriden on the specific type of '{type}'!");
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
            var asyncSubs = MachineBase.CurrentMachine.EventSystem.PEvents.AsyncSubscribers;
            foreach (var subs in asyncSubs)
            {
                subs.Value.Remove(this);
            }
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
                ToolBase,
                parameters
            }) as ICommand;
            instance.Family = Family;
            return instance;
        }

        public override string ToString()
        {
            lock (Builder)
            {
                Builder.Clear();
                Builder.Append(GetType().Name);
                foreach (var value in Parameters.Values)
                {
                    if (!Single.IsNaN(value.Value))
                    {
                        Builder.Append(' ');
                        if (value.Key == "Y")
                        {
                            Builder.Append("Z");
                        }
                        else if (value.Key == "Z")
                        {
                            Builder.Append("Y");
                        }
                        else
                        {
                            Builder.Append(value.Key);
                        }

                        Builder.Append(value.Value);
                    }
                }
                
                return Builder.ToString();
            }
        }
        /// <summary>
        /// Up for changes in the future, most likely a conversion from ExecuteFinal to Execute.
        /// </summary>
        public async Task OnEventInvoked() => await ExecuteFinal(false);

        public static BaseCommand Create(Type typeOfCommand, ICommandParameters optionalParameters = null)
        {
            var command = Activator.CreateInstance(typeOfCommand, Globals.Tool, optionalParameters);

            return command as BaseCommand;
        }

        public static T Create<T>(ICommandParameters optionalParameters = null)
        {
            var command = Activator.CreateInstance(typeof(T), Globals.Tool, optionalParameters);

            return (T) command;
        }
        
        private static readonly StringBuilder Builder = new StringBuilder(); 
    }
}
