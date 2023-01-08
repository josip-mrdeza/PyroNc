using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory
{
    public ref struct StackPointer<T>  where T: unmanaged
    {
        private IntPtr _val;
        private bool _isReadonly;
        public T Value
        {
            get
            {
                unsafe
                {
                    var ptr = (T*) _val.ToPointer();

                    return *ptr;
                }
            }
            set
            {
                if (_isReadonly)
                {
                    throw new NotSupportedException("This FixedPointer was created in a read-only mode!");
                }
                unsafe
                {
                    T* ptr = (T*)_val.ToPointer();
                    *ptr = value;
                }
            }
        }

        public unsafe T* Pointer => (T*) _val;

        public void SubscribeToChanges(EventHandler<IntPtr> handler)
        {
            InternalStackPointerChecker.Subscribe(_val, handler);
        }

        public void Dispose()
        {
            InternalStackPointerChecker.UnsubscribeAllFor(_val);
        }

        public static unsafe StackPointer<T> CreateByReadonlyReference(in T obj)
        {
            StackPointer<T> sp = new StackPointer<T>();
            fixed (T* ptr = &obj)
            {
                sp._val = new IntPtr(ptr);
            }

            sp._isReadonly = true;
            return sp;
        }

        public static unsafe StackPointer<T> CreateByReference(in T obj)
        {
            StackPointer<T> sp = new StackPointer<T>();
            fixed (T* ptr = &obj)
            {
                sp._val = new IntPtr(ptr);
            }
            return sp;
        }

        public static StackPointer<T> CreateFromPointer(IntPtr ptr)
        {
            StackPointer<T> sp = new StackPointer<T>();
            sp._val = ptr;
            return sp;
        }
        
        public static unsafe StackPointer<T> CreateFromPointer(T* ptr)
        {
            StackPointer<T> sp = new StackPointer<T>();
            sp._val = new IntPtr(ptr);
            return sp;
        }
        
        public static unsafe StackPointer<T> CreateFromPointer(void* ptr)
        {
            StackPointer<T> sp = new StackPointer<T>();
            sp._val = new IntPtr(ptr);
            return sp;
        }
    }
}