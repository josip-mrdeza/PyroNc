using System.Threading.Tasks;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G54 : BaseCommand
    {
        public G54(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
            
        }

        public override async Task Execute(bool draw)
        {
            //??
            ToolBase.Values.WorkOffsets[0] = new Vector3(Parameters.GetValue("X"), Parameters.GetValue("Y"), Parameters.GetValue("Z"));
            await Machine.EventSystem.PEvents.FireAsync("WorkOffsetChange_0");
        }
    }
}