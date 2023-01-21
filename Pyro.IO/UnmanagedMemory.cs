using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pyro.IO;

public class UnmanagedMemory<T> : IDisposable
{
    public IntPtr Pointer => _didInit ? _pointer : throw new NotSupportedException("Unmanaged memory has not been assigned yet!");
    public T Value
    {
        get => Marshal.PtrToStructure<T>(_pointer);
    }
    private IntPtr _pointer;
    private readonly bool _didInit;
    private bool _freedMemory;
    private long _size;
    private long _length;
    private bool _isArray;
    private static MemberInfo[] StructureInfo;

    public UnmanagedMemory(T value, long size, bool isArray = false, long length = 0)
    {
        Setup(size, length);
        if (isArray)
        {
            _isArray = true;
            unsafe
            {
                Unsafe.Write(_pointer.ToPointer(), value);
            }
            _didInit = true;
            return;
        }
        Marshal.StructureToPtr(value, _pointer, false);
        _didInit = true;
    }

    public UnmanagedMemory(long size, bool isArray = false, long length = 0)
    {
        Setup(size, length);
        if (StructureInfo is null)
        {
            StructureInfo = typeof(T).GetMembers();
        }
        //TODO make this work for structs with multiple correlated items of the same type.
        throw new NotImplementedException();
    }
    
    private void Setup(long size, long length)
    {
        _size = size;
        _length = length == 0 ? size : length;
        _pointer = Marshal.AllocHGlobal((IntPtr)length);
    }
    public void Write(T value, int offset)
    {
        if (_isArray)
        {
            unsafe
            {
                Unsafe.Write((void*) IntPtr.Add(_pointer, offset), value);
            }
        }
    }
    
    public T this[int el]
    {
        get
        {
            if (!_isArray)
            {
                throw new NotSupportedException("Memory does not point to an array!");
            }

            var ptr = IntPtr.Add(_pointer, (int) (el * _size));

            unsafe
            {
                return Unsafe.Read<T>(ptr.ToPointer());
            }
        }

        set
        {
            if (!_isArray)
            {
                throw new NotSupportedException("Memory does not point to an array!");
            }

            var ptr = IntPtr.Add(_pointer, (int)(el * _size));

            unsafe
            {
                Unsafe.Write(ptr.ToPointer(), value);
            }
        }
    }

    public void FreeMemory()
    {
        if (!_freedMemory)
        {
            InternalFreeMemory();
        }
    }
    public void Dispose()
    {
        FreeMemory();
        GC.SuppressFinalize(this);
    }

    internal void InternalFreeMemory()
    {
        var ptr = Pointer;
        Marshal.FreeHGlobal(ptr);
        _pointer = IntPtr.Zero;
        _freedMemory = true;
    }
}