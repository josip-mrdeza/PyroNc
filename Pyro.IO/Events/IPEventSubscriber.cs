using System.Threading.Tasks;

namespace Pyro.IO.Events
{
    public interface IPEventSubscriber
    {
        public void OnEventInvoked();
    }
    
    public interface IPEventSubscriber<in T> : IPEventSubscriber
    {
        public void OnEventInvoked(T obj);
    }
    
    public interface IPEventSubscriber<in T, in T2> : IPEventSubscriber
    {
        public void OnEventInvoked(T obj1, T2 obj2);
    }
    
    public interface IPEventSubscriber<in T, in T2, in T3> : IPEventSubscriber
    {
        public void OnEventInvoked(T obj1, T2 obj2, T3 obj3);
    }
}