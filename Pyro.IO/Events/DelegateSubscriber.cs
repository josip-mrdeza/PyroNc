using System;
using System.Threading.Tasks;

namespace Pyro.IO.Events;

public class DelegateSubscriber : IPAsyncEventSubscriber
{
    private Func<Task> dg;
    public DelegateSubscriber(Func<Task> func)
    {
        dg = func;
    }

    public async Task OnEventInvoked()
    {
        await dg();
    }
}