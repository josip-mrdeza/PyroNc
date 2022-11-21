using System.Threading.Tasks;

namespace Pyro.Nc.Configuration
{
    public interface IManager
    {
        public bool IsAsync { get; }
        public bool DisableAutoInit { get; }
        public void Init();
        public Task InitAsync();
    }
}