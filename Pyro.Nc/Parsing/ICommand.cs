using System;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing
{
    public interface ICommand : IShallowCommand
    {
        public bool IsModal { get; }
        public bool IsArc { get; }
        public string AdditionalInfo { get; set; }
        public Guid Id { get; }
        public ICommandParameters Parameters { get; set; }
        public void UpdateCurrent();
        public Task Execute(bool draw);
        public Task ExecuteTurning(bool draw);
        public Task ExecuteFinal(bool draw);
        public void Expire();
        public void Plan();
        public void Cancel();
        public void Renew();
        public ICommand Copy();
    }
}