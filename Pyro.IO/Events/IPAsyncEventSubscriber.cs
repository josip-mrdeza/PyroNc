using System.Threading.Tasks;

namespace Pyro.IO.Events
{
    public interface IPAsyncEventSubscriber
    {
        public Task OnEventInvoked();
    }
    
    public interface IPAsyncEventSubscriber<in T> : IPAsyncEventSubscriber
    {
        public Task OnEventInvoked(T obj);
    }
    
    public interface IPAsyncEventSubscriber<in T, in T2> : IPAsyncEventSubscriber
    {
        public Task OnEventInvoked(T obj1, T2 obj2);
    }
    
    public interface IPAsyncEventSubscriber<in T, in T2, in T3> : IPAsyncEventSubscriber
    {
        public Task OnEventInvoked(T obj1, T2 obj2, T3 obj3);
    }
}