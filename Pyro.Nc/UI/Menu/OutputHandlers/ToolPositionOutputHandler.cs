using System;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI.Menu.OutputHandlers
{
    public class ToolPositionOutputHandler : OutputHandler
    {
        private void Update()
        {
            if (Globals.Tool is null)
            {
                return;
            }
            ValueText.text = Globals.Tool.Position.ToString();
        }
    }
}