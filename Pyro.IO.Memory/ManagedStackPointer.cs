using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory;

public ref struct ManagedStackPointer<T> where T : struct
{
     private IntPtr _val;
     private bool _isReadonly;
     private bool _isUnmanaged;

     public T Value
     {
          get => Marshal.PtrToStructure<T>(_val);
     }

     public ManagedStackPointer()
     {
          
     }
}