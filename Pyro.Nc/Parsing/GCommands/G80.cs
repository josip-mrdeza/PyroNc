using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G80 : BaseCommand
    {
        public G80(ITool tool, GCommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.G80;

        public override async Task Execute(bool draw)
        {
            if (Tool.Values.Current.GetType().Name is nameof(G81))
            {
                Tool.Values.TokenSource.Cancel();
                Tool.Values.TokenSource.Dispose();
                Tool.Values.TokenSource = new CancellationTokenSource();
                await Tool.Traverse(Tool.Position.Mutate(p =>
                {
                    p.y = 0;

                    return p;
                }), LineTranslationSmoothness.Rough, false);
            }
        }
    }
}