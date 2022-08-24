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
        public ICommandParameters Parameters { get; set; }
        public void UpdateCurrent();
        public Task Execute(bool draw);
        public Task ExecuteFinal(bool draw);
        public void Expire();
        public void Plan();
        public ICommand Copy();
    }
}