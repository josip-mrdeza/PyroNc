using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pyro.IO.Memory
{
    public static class MainHeap
    {
        public static IntPtr AccessMemory(string processName)
        {
            var mem = Process.GetCurrentProcess();
            var ptr = mem.MainModule.;

            return ptr;
        }

        public static long Read(this IntPtr ptr)
        {
            return Marshal.(ptr).ToInt64();
        }
    }
}