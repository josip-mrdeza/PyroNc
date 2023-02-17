using Pyro.IO;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;

namespace Pyro.Nc.UI.Debug;

public class RadiusDebugView : LineViewer
{
    private ToolBase _toolBase;
    private string Name = "RadiusDebug.enabled";
    private void Start()
    {
        base.Init();
        _toolBase = GetComponent<ToolBase>();
        var roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
        if (roaming.Exists(Name))
        {
            IsActive = roaming.ReadFileAs<bool>(Name);
        }
        else
        {
            roaming.AddFile(Name, false);
            IsActive = false;
        }
    }

    public override void Update()
    {
        if (!IsActive)
        {
            return;
        }
        IsDirty = true;
        Radius = _toolBase.Values.Radius;
        base.Update();
    }
}