namespace Pyro.Nc.UI
{
    public class View3D : View
    {
        public override void Show()
        {
            base.Show();
            ViewHandler.Active = false;
        }
    }
}