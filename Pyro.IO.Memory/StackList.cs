using System;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory;

public ref struct StackList<T> where T : struct
{
    public ManagedStackPointer<T> Pointer { get; }
    private readonly int _sizeOfElement = Marshal.SizeOf<T>();
    private Span<byte> _span;
    public StackList()
    {
        Pointer = new ManagedStackPointer<T>();
    }
}