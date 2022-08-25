using System.Threading.Tasks;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G82 : G81
    {
        public G82(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
            if (parameters.Values.ContainsKey("P"))
            {
                Milliseconds = (int) parameters.Values["P"];
            }
        }
        
        public int Milliseconds { get; set; }

        public override async Task Execute(bool draw)
        {
            await base.Execute(draw);
            await Task.Delay(Milliseconds, Parameters.Token);
        }
    }
}