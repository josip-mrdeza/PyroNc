using System.Threading.Tasks;

namespace Pyro.IO.Events
{
    public interface IPAsyncEventSubscriber
    {
        public Task Execute();
    }
    
    public interface IPAsyncEventSubscriber<in T> : IPAsyncEventSubscriber
    {
        public Task Execute(T obj);
    }
    
    public interface IPAsyncEventSubscriber<in T, in T2> : IPAsyncEventSubscriber
    {
        public Task Execute(T obj1, T2 obj2);
    }
    
    public interface IPAsyncEventSubscriber<in T, in T2, in T3> : IPAsyncEventSubscriber
    {
        public Task Execute(T obj1, T2 obj2, T3 obj3);
    }
}