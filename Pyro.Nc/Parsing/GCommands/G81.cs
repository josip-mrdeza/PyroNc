using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G81 : G01
    {
        public G81(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override async Task Execute(bool draw)
        {
            var height = (Parameters as GCommandParameters).Y;
            await Tool.Traverse(Tool.Position.Mutate(p =>
            {
                p.y = height;

                return p;
            }), LineTranslationSmoothness.Rough, draw);
        }
    }     
}