using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;

namespace Pyro.Nc.Simulation.Tools;

public class ToolControl
{
    public ToolManager Manager { get; }
    public ToolBase SelectedTool => Globals.Tool;

    public ToolControl()
    {
        Manager = new ToolManager();
        Manager.Init();
    }
}