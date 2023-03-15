using System.Threading.Tasks;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.SyntacticalCommands;

public class ENDFORLOOP : BaseCommand
{
    public ENDFORLOOP() : base(Globals.Tool, new ArbitraryCommandParameters())
    {
    }

    public override string Description => "Ends the for loop context.";

    public override async Task Execute(bool draw)
    {
        
    }
}