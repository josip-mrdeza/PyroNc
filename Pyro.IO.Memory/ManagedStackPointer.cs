using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory;

public readonly ref struct ManagedStackPointer
{
     internal readonly IntPtr _val;
     internal readonly bool _isReadonly;
     internal readonly bool _isUnmanaged;
     internal unsafe void** ArrayPointer
     {
          get
          {
               if (!IsArray)
               { 
                    throw new Exception("Instance is not declared as a pointer to an array.");
               }
               return (void**)_val;
          }
     }

     public bool IsArray { get; }
     public unsafe void* this[int index] => ArrayPointer[index];

     public T GetValue<T>() where T : struct
     {
          if (_val == IntPtr.Zero)
          {
               throw new Exception($"Value pointer was null (IntPtr.Zero), '{_val.ToString()}'!");
          }
          return Marshal.PtrToStructure<T>(_val);   
     }

     public void SetValue<T>(T value) where T : struct
     {
          if (_isReadonly)
          {
               throw new Exception($"Cannot write to {(_isUnmanaged ? "an unmanaged " : "a managed ")}readonly pointer '{_val.ToString()}'!");
          }
          Marshal.StructureToPtr(value, _val, true);
     }

     public void SetValueRef<T>(ref T rValue) where T : struct
     {
          if (_isReadonly)
          {
               throw new Exception($"Cannot write to {(_isUnmanaged ? "an unmanaged " : "a managed ")}readonly pointer '{_val.ToString()}'!");
          }
          Marshal.StructureToPtr(rValue, _val, true);
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

     public unsafe ManagedStackPointer(void** arrPtr, bool isReadonly = false)
     {
          _val = new IntPtr(arrPtr);
          isReadonly = _isReadonly;
          IsArray = true;
     }
     
     public static unsafe implicit operator void*(ManagedStackPointer ptr)
     {
          return (void*) ptr._val;
     }

     public static unsafe implicit operator ManagedStackPointer(void* ptr)
     {
          return new ManagedStackPointer(ptr);
     }
}

public readonly ref struct ManagedStackPointer<T> where T : struct
{
     public ManagedStackPointer Pointer { get; }
     public T Value
     {
          get => Pointer.GetValue<T>();
          set => Pointer.SetValue(value);
     }

     public ManagedStackPointer(IntPtr ptr, bool isReadonly = false)
     {
          Pointer = new ManagedStackPointer(ptr, isReadonly);
     }
     
     public unsafe ManagedStackPointer(void* ptr, bool isReadonly = false)
     {
          Pointer = new ManagedStackPointer(ptr, isReadonly);
     }
     
     public static unsafe implicit operator void*(ManagedStackPointer<T> mptr)
     {
          return (void*) mptr.Pointer._val;
     }

     public static unsafe implicit operator ManagedStackPointer<T>(void* ptr)
     {
          return new ManagedStackPointer<T>(ptr);
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