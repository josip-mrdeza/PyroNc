using System;
using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Comment : BaseCommand
    {
        public Comment(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            
        }
        public string Text { get; set; }
        public override string Description => Text;

        public override Task Execute(bool draw)
        {
            Globals.Console.PushText(Description);

            return Task.CompletedTask;
        }
    }
}