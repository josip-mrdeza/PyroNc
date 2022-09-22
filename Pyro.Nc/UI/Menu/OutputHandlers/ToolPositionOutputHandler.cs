using System;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI.Menu.OutputHandlers
{
    public class ToolPositionOutputHandler : OutputHandler
    {
        private void Update()
        {
            ValueText.text = Globals.Tool.Position.ToString();
        }
    }
}