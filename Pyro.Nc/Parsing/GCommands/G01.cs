using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G01 : G00
    {
        public G01(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            Tool.ThrowNoToolException();
            if(Tool.Values.FeedRate == 0)
            {
                throw new FeedRateNotDefinedException();
            }
            if (Tool.Values.SpindleSpeed == 0)
            {
                throw new SpindleSpeedNotDefinedException();
            }
            await base.Execute(draw);
        }

        public override string Description => Locals.G01;
    }
}
