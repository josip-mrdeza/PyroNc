using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G82 : G81
    {
        public G82(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
            if (parameters.Values.ContainsKey("P"))
            {
                Milliseconds = (int) parameters.Values["P"];
            }
        }
        public override string Description => Locals.G82;
        public int Milliseconds { get; set; }

        public override async Task Execute(bool draw)
        {
            await base.Execute(draw);
            await Task.Delay(Milliseconds, Parameters.Token);
        }
    }
}