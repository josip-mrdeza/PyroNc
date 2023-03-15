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
            AdditionalInfo = $"\n{Machine.SimControl.WorkOffsets[0].ToString()}";
        }

        public override async Task Execute(bool draw)
        {
            Machine.SimControl.WorkOffset = Machine.SimControl.WorkOffsets[0];
        }
    }
}