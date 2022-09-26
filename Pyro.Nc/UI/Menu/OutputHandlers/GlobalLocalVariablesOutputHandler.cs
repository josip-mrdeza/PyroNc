using System.Diagnostics;
using System.Globalization;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI.Menu.OutputHandlers
{
    public class GlobalLocalVariablesOutputHandler : OutputHandler
    {
        Stopwatch _stopwatch = Stopwatch.StartNew();
        private void Update()
        {
            if (Globals.Roaming is null)
            {
                ValueText.text = $"Null - {_stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)}s";
            }
            else
            {
                ValueText.text = "Instance";
                _stopwatch.Stop();
            }
        }
    }
}