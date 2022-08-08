using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G61 : G00
    {
        public G61(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            await Tool.Storage.FetchGCommand("G00").Mutate(c =>
            {
                var mem = MemorySlots.Get((int) Parameters.GetValue("S"));
                var cparams = c.Parameters;
                cparams.AddValue("X", mem.x);
                cparams.AddValue("Y", mem.y);
                cparams.AddValue("Z", mem.z);
                return c;
            }).Execute(draw);
        }
    }
}