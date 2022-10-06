using System.Threading;
using Pyro.IO;
using Pyro.Nc.Parsing;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Simulation
{
    public static class ToolHelper
    {
        public static ToolValues GetDefaultsOrCreate(this ITool tool)
        {
            return Globals.DefaultsManager.Values.Mutate(x =>
            {
                x.Storage = ValueStorage.CreateFromFile(tool);
                x.TokenSource = new CancellationTokenSource();
                return x;
            }) ?? new ToolValues(tool);
        }
    }
}