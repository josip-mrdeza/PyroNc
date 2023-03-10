using System;
using Android.OS;
using Java.Interop;
using Java.Lang;

namespace PyroController
{
    public class QueueHandler : Java.Lang.Object, IRunnable
    {
        public void SetJniIdentityHashCode(int value)
        {
        }

        public void SetPeerReference(JniObjectReference reference)
        {
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
        }

        public void DisposeUnlessReferenced()
        {
            throw new NotImplementedException();
        }

        public void Disposed()
        {
        }

        public void Finalized()
        {
        }

        public void Run()
        {
            MainActivity.Queue.ThreadHandle().GetAwaiter().GetResult();
        }
    }
}