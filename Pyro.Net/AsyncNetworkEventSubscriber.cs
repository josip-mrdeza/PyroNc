namespace Pyro.Net
{
    public abstract class AsyncNetworkEventSubscriber : NetworkEventSubscriber
    {
        public AsyncNetworkEventSubscriber()
        {
            IsAsync = true;
        }
    }
}