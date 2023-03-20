using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G01 : G00
    {
        public G01(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            ToolBase.ThrowNoToolException();
            var spindle = Machine.SpindleControl;
            if(spindle.FeedRate == 0)
            {
                throw new FeedRateNotDefinedException($"~[Line {Line}]:Can't move tool with no feed rate defined!~");
            }
            if (spindle.SpindleSpeed == 0)
            {
                throw new SpindleSpeedNotDefinedException();
            }
            await base.Execute(draw);
        }
        

        public override string Description => Locals.G01;
    }
}
