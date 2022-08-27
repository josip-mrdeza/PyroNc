using System;
using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Comment : BaseCommand
    {
        public Comment(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            
        }
        public string Text { get; set; }
        public override string Description => Text;

        public override async Task Execute(bool draw)
        { 
            //TODO Make it show up as a message
            Console.WriteLine(Description);
        }
    }
}