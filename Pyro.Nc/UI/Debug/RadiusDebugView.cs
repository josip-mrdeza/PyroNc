using Pyro.IO;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI.Debug;

public class RadiusDebugView : LineViewer
{
    private ITool Tool;
    private string Name = "RadiusDebug.enabled";
    private void Start()
    {
        base.Init();
        Tool = GetComponent<ITool>();
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
        IsDirty = true;
        Radius = Tool.Values.Radius;
        base.Update();
    }
}