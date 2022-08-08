using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public interface ICommand
    {
        public ITool Tool { get; set; }
        public bool IsModal { get; }
        public bool IsArc { get; }
        public string Description { get; }
        public ICommandParameters Parameters { get; set; }
        public Task Execute();
        public Task Execute(bool draw);
        public void Expire();
        public void Plan();
        public ICommand Copy();
    }
}