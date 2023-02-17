using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.Parsing.GCommands
{
    public sealed class G80 : BaseCommand
    {
        public G80(ToolBase toolBase, GCommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.G80;

        public override async Task Execute(bool draw)
        {
            if (Machine.Runner.CurrentContext.GetType().Name is nameof(G81))
            {
                ToolBase.Values.TokenSource.Cancel();
                ToolBase.Values.TokenSource.Dispose();
                ToolBase.Values.TokenSource = new CancellationTokenSource();
                await ToolBase.Traverse(ToolBase.Position.Mutate(p =>
                {
                    p.y = 0;

                    return p;
                }), LineTranslationSmoothness.Rough, false);
            }
        }
    }
}