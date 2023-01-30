using System.Threading.Tasks;

namespace Pyro.Net
{
    public abstract class NetworkEventSubscriber
    {
        public bool IsAsync { get; private protected set; }
        public abstract void OnEvent();
        public abstract Task OnEventAsync();
    }
}