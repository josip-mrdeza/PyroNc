using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.Cycles;

public class CYCLE82 : CYCLE81
{
    public CYCLE82(ToolBase toolBase, ICommandParameters parameters, float[] splitParameters) : base(toolBase, parameters, splitParameters)
    {
    }
}