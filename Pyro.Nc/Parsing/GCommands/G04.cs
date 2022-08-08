using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G04 : G00
    {
        public G04(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            await Tool.Storage.FetchMCommand("M00").Mutate(m =>
            {
                var flag0 = Parameters.Values.TryGetValue("S", out var ms);
                ms *= 1000f;
                if (!flag0)
                {
                    Parameters.Values.TryGetValue("P", out ms);
                }
                m.Parameters.AddValue("P", ms);
                return m;
            }).Execute(draw);
        }
    }
}