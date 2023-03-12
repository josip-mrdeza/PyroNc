using System.Threading.Tasks;

namespace Pyro.Nc.Configuration;

public interface IPyroPlugin
{
    public void InitializePlugin();

    public void Update();
}