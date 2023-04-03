namespace Pyro.Nc.UI.ToolsView;

public class ToolsView : View
{
    public static ToolsView Instance;
    public override void Initialize()
    {
        base.Initialize();
        Instance = this;
    }
}