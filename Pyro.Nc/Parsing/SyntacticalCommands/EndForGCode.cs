using System.Threading.Tasks;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class EndForGCode : BaseCommand
{
    public EndForGCode() : base(Globals.Tool, new ArbitraryCommandParameters())
    {
    }

    public override async Task Execute(bool draw)
    {
        
    }
}