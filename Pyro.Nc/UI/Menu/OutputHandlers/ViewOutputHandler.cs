using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI.Menu.OutputHandlers
{
    public class ViewOutputHandler : OutputHandler
    {
        private void Update()
        {
            ValueText.text = ViewHandler.Active.ToString();
        }
    }
}