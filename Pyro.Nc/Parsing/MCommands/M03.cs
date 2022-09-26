using System;
using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M03 : BaseCommand
    {
        public M03(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }
        public override string Description => Locals.M03;

        public override async Task Execute(bool draw)
        { 
            //TODO make it change the spindle clockwise rotation speed...
            Tool.Values.SpindleSpeed.Set(Parameters.GetValue("S"));
            Tool.Values.FeedRate.Set(Parameters.GetValue("F"));
        }
    }
}