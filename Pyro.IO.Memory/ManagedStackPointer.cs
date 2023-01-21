using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory;

public ref struct ManagedStackPointer<T> where T : struct
{
     private IntPtr _val;
     private readonly bool _isReadonly;
     private bool _isUnmanaged;

     public T Value
     {
          get
          {
               if (_val == IntPtr.Zero)
               {
                    throw new Exception($"Value pointer was null (IntPtr.Zero), '{_val.ToString()}'!");
               }
               return Marshal.PtrToStructure<T>(_val);
          }
          set
          {
               if (_isReadonly)
               {
                    throw new Exception($"Cannot write to {(_isUnmanaged ? "an unmanaged " : "a managed ")}readonly pointer '{_val.ToString()}'!");
               }
               Marshal.StructureToPtr(value, _val, true);
          }
     }
     public ManagedStackPointer(IntPtr ptr, bool isReadonly = false)
     {
          _val = ptr;
          _isReadonly = isReadonly;
     }

     public unsafe ManagedStackPointer(void* ptr, bool isReadonly = false)
     {
          _val = new IntPtr(ptr);
          _isReadonly = isReadonly;
     }

     public static unsafe ManagedStackPointer<TUnmanaged> GetStackPointer<TUnmanaged>(TUnmanaged* pointer, bool isReadonly = false) where TUnmanaged : unmanaged
     {
          ManagedStackPointer<TUnmanaged> msp = new ManagedStackPointer<TUnmanaged>(pointer, isReadonly);

          return msp;
     }
     
     public static unsafe ManagedStackPointer<TUnmanaged> GetStackPointer<TUnmanaged>(ref TUnmanaged value) where TUnmanaged : unmanaged
     {
          fixed (TUnmanaged* ptr = &value)
          {
               ManagedStackPointer<TUnmanaged> msp = new ManagedStackPointer<TUnmanaged>(ptr);
               return msp;
          }
     }
     
     public static unsafe ManagedStackPointer<TUnmanaged> GetStackPointerReadonly<TUnmanaged>(in TUnmanaged value) where TUnmanaged : unmanaged
     {
          fixed (TUnmanaged* ptr = &value)
          {
               ManagedStackPointer<TUnmanaged> msp = new ManagedStackPointer<TUnmanaged>(ptr, true);
               return msp;
          }
     }
}