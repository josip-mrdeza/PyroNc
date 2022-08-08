using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M00 : ICommand
    {
        public M00(ITool tool, ICommandParameters parameters)
        {
            Tool = tool;
            Parameters = parameters;
        }
        public ITool Tool { get; set; }
        public virtual bool IsModal => false;
        public virtual bool IsArc => false;
        public string Description { get; }
        public ICommandParameters Parameters { get; set; }

        public async Task Execute()
        {
            await Execute(false);
        }
        public virtual async Task Execute(bool draw)
        {
            var flag0 = Parameters.Values.TryGetValue("S", out var ms);
            ms *= 1000f;
            if (!flag0)
            {
                Parameters.Values.TryGetValue("P", out ms);
            }

            var parameters = Parameters as MCommandParameters;
            for (int i = 0; i < ms; i++)
            {
                await Task.Yield();
                if (parameters!.Token.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(1);
                Tool.IsAllowed = false;
            }

            Tool.IsAllowed = true;
        }
        public void Expire()
        {
            throw new NotSupportedException();
        }
        public void Plan()
        {
            throw new NotSupportedException(); 
        }
        public virtual ICommand Copy()
        {
            return this.MemberwiseClone() as ICommand;
        }
    }
}