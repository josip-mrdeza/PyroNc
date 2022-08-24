using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G80 : BaseCommand
    {
        public G80(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.G80;

        public override async Task Execute(bool draw)
        {
            if (Tool.Current.GetType().Name is nameof(G81))
            {
                 Tool.TokenSource.Cancel();
                 Tool.TokenSource.Dispose();
                 Tool.TokenSource = new CancellationTokenSource();
                 await Tool.Traverse(Tool.Position.Mutate(p =>
                 {
                     p.y = 0;

                     return p;
                 }), LineTranslationSmoothness.Rough, false);
            }
        }
    }
}